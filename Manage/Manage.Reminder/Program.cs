using Asp.Versioning;
using Manage.Data.Public;
using Manage.Data.Reminder;
using Manage.Data.Reminder.Repository;
using Manage.Reminder.Background;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.Services.AddDbContext<Context>(options => options
                        .UseNpgsql(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")));

builder.Services.AddScoped<ICache>(provider => new CacheService(Environment.GetEnvironmentVariable("REDIS_CONNECTION")));

// Respository scopes
builder.Services.AddScoped<ISubscription, SubscriptionEF>();
builder.Services.AddScoped<IUserSubscription, UserSubscriptionEF>();
builder.Services.AddScoped<IReminder, ReminderEF>();


builder.Services.AddHostedService<Remind>(provider => new Remind(
        provider.GetRequiredService<ILogger<Remind>>(),
        provider,
        Environment.GetEnvironmentVariable("gRPCUser")
    ));



builder.Services.AddSingleton<IFile>(provider => new Files(
    Environment.GetEnvironmentVariable("MINIO_CONNECTION"),
    Environment.GetEnvironmentVariable("MINIO_ACCESSKEY"),
    Environment.GetEnvironmentVariable("MINIO_SECRETKEY"),
    true
));

builder.Services.AddSingleton<ISendMail>(provider => new SendMail(
    Environment.GetEnvironmentVariable("EMAIL_CREDENTIALS"),
    Environment.GetEnvironmentVariable("EMAIL_ADDRESS"),
    Environment.GetEnvironmentVariable("EMAIL_HOST"),
    Environment.GetEnvironmentVariable("EMAIL_NAME")
));

builder.Services.AddSingleton<ISMS>(provider => new SMS(
    Environment.GetEnvironmentVariable("SMS_USERNAME"),
    Environment.GetEnvironmentVariable("SMS_PASSWORD"),
    Environment.GetEnvironmentVariable("SMS_PHONENUMBER")
));


// versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Version")
    );
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});


builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var modelState = actionContext.ModelState.Values;
        return new BadRequestObjectResult(new { message = "Fill out all required fields in right format" });
    };
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(s =>
{
    s.EnableAnnotations();
    s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                          Enter 'Bearer' [space] and then your token in the text input below.
                          \r\n\r\nExample: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    s.AddSecurityRequirement(new OpenApiSecurityRequirement()
      {
        {
          new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference
              {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
              },
              Scheme = "oauth2",
              Name = "Bearer",
              In = ParameterLocation.Header,
            },
            new List<string>()
          }
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{

//}
app.UseSwagger();
app.UseSwaggerUI();

// Handling 5xx response code
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        if (context.Response.StatusCode / 100 == 5)
        {
            await context.Response.WriteAsJsonAsync(new { message = "Server-Error", error = context.Features.Get<IExceptionHandlerPathFeature>()?.Error.Message });
        }
    });
});

// Handling 401,403 response code
app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == 401 || context.HttpContext.Response.StatusCode == 403)
    {
        await context.HttpContext.Response.WriteAsJsonAsync(new { message = "Forbidden" });
    }
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
