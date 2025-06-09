using Manage.Data.Identity;
using Manage.Data.Identity.Repository;
using Manage.gRPC.Identity.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.Services.AddDbContext<Context>(options => options
                        .UseNpgsql(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")));

builder.Services.AddScoped<ICache>(provider => new CacheService(Environment.GetEnvironmentVariable("REDIS_CONNECTION")));

// Respository scopes
builder.Services.AddScoped<IUser, UserEF>();
builder.Services.AddScoped<IAccess, AccessEF>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<UserService>();
app.MapGrpcService<AccessService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
