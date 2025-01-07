using Asp.Versioning;
using Manage.Data.Management;
using Manage.Data.Management.Repository;
using Manage.Data.Public;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<Context>(options => options
                        .UseNpgsql(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")));

// Respository scopes
builder.Services.AddScoped<ITenant, TenantEF>();
builder.Services.AddScoped<IDomain, DomainEF>();
builder.Services.AddScoped<IDomainValue, DomainValueEF>();
builder.Services.AddScoped<ISubDomainValue, SubDomainValueEF>();
builder.Services.AddScoped<IUser, UserEF>();
builder.Services.AddScoped<IAction, ActionEF>();
builder.Services.AddScoped<IActionGroup, ActionGroupEF>();
builder.Services.AddScoped<IRACC, RACCEF>();
builder.Services.AddScoped<IRole, RoleEF>();
builder.Services.AddScoped<IService, ServiceEF>();
builder.Services.AddScoped<IUACC, UACCEF>();
builder.Services.AddScoped<IUserRole, UserRoleEF>();

builder.Services.AddScoped<ICache>(provider => new CacheService(Environment.GetEnvironmentVariable("REDIS_CONNECTION")));

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


// Authorization policies
builder.Services.AddAuthorization(options =>
{
    var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                JwtBearerDefaults.AuthenticationScheme);
    defaultAuthorizationPolicyBuilder =
    defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
    options.AddPolicy("realDelete", policy => policy.RequireClaim("realDelete", "1"));
});

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var modelState = actionContext.ModelState.Values;
        return new BadRequestObjectResult(new { message = "Fill out all required fields in right format" });
    };
});

// Authentication schema
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY")))
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
