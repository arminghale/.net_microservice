using Manage.Data.Identity;
using Manage.Data.Identity.DTO.General;
using Manage.Data.Identity.Models;
using Manage.Data.Identity.Repository;
using Manage.Identity.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Manage.Identity.Test
{
    public class AuthIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly IServiceProvider _services;

        public AuthIntegrationTest(WebApplicationFactory<Program> factory)
        {
            _services = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace with test DB
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<Context>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<Context>(options =>
                    {
                        options.UseNpgsql(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION"));
                    });
                    services.AddScoped<AuthApiController>();
                });
            }).Services;
        }

        [Fact]
        public async Task Login_ReturnsExpectedResult()
        {
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Context>();

            // Seed DB
            if (!context.User.Any(u => u.Username == "admin"))
            {
                context.User.Add(new User { Username = "admin", Password = "admin" });
                context.SaveChanges();
            }

            var controller = scope.ServiceProvider.GetRequiredService<AuthApiController>();
            var valid = new PostLogin { username = "admin", password = "admin" };
            var invalid = new PostLogin { username = "admin", password = "wrong" };

            var success =await  controller.Login(1, valid);
            var failure = await controller.Login(1, invalid);

            Assert.IsType<ContentResult>(success);
            Assert.IsType<BadRequestObjectResult>(failure);
        }
    }

    public class AuthE2ETest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AuthE2ETest(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace DB with test DB
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<Context>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<Context>(options =>
                    {
                        options.UseNpgsql(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION"));
                    });

                    // Optional: Seed DB
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<Context>();
                    db.Database.EnsureCreated();
                    if (!db.User.Any(u => u.Username == "admin"))
                    {
                        db.User.Add(new User { Username = "admin", Password = "admin" });
                        db.SaveChanges();
                    }
                });
            });
        }

        [Fact]
        public async Task Login_EndToEnd_ReturnsExpectedResult()
        {
            var client = _factory.CreateClient();

            var successInput = new { username = "admin", password = "admin" };
            var failInput = new { username = "admin", password = "wrong" };

            var successRes = await client.PostAsJsonAsync("/api/v1/authapi/1/login", successInput);
            var failRes = await client.PostAsJsonAsync("/api/v1/authapi/1/login", failInput);

            Assert.True(successRes.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, failRes.StatusCode);
        }
    }

}
