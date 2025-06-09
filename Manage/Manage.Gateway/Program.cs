using Manage.Gateway.Services;
using Manage.Identity.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<ICache>(provider => new CacheService(Environment.GetEnvironmentVariable("REDIS_CONNECTION")));

builder.Services.AddScoped<Manage.Identity.Middlewares.IAuthorizationService, AuthorizationService>(
    provider => new AuthorizationService(provider.GetService<ILogger<AuthorizationService>>(), provider));



// Authentication schema
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
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

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                JwtBearerDefaults.AuthenticationScheme);
    defaultAuthorizationPolicyBuilder =
    defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
});

builder.Services.AddControllers();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot();

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


await app.UseOcelot(new OcelotPipelineConfiguration
{
    AuthorizationMiddleware = async (context, next) =>
    {
        var middleware = context.RequestServices.GetService<Manage.Identity.Middlewares.IAuthorizationService>();
        bool authorized = await middleware.CheckUserAuthorizationAsync(context);
        if (!authorized)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }
        await next.Invoke();
    }
});

app.Run();
