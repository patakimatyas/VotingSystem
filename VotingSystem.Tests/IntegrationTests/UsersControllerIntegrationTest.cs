using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using VotingSystem.WebAPI;
using VotingSystem.DataAccess;
using VotingSystem.DataAccess.Models;
using VotingSystem.Shared.DTOs;
using Microsoft.AspNetCore.Hosting;

namespace VotingSystem.Tests.IntegrationTests
{
    public class UsersControllerIntegrationTests
        : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public UsersControllerIntegrationTests()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "IntegrationTest");

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("IntegrationTest");
                    builder.ConfigureServices(services =>
                    {
                        // Replace real DB with in-memory
                        var desc = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<VotingSystemDbContext>));
                        if (desc != null) services.Remove(desc);

                        services.AddDbContext<VotingSystemDbContext>(opts =>
                            opts.UseInMemoryDatabase(_dbName));

                        // Seed a test user for login/logout
                        using var scope = services
                            .BuildServiceProvider()
                            .CreateScope();
                        var sp = scope.ServiceProvider;
                        var db = sp.GetRequiredService<VotingSystemDbContext>();
                        var uMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();

                        db.Database.EnsureCreated();
                        SeedUser(uMgr).Wait();
                    });
                });

            _client = _factory.CreateClient();
        }

        #region Register

        [Fact]
        public async Task Register_ReturnsCreated_WhenValid()
        {
            var dto = new UserRequestDTO
            {
                Email = "new@user.com",
                Name = "newuser",
                Password = "Password1!"
            };

            var res = await _client.PostAsJsonAsync("/api/users/register", dto);
            Assert.Equal(HttpStatusCode.Created, res.StatusCode);

            var created = await res.Content.ReadFromJsonAsync<UserResponseDTO>();
            Assert.Equal(dto.Email, created?.Email);
            Assert.Equal(dto.Name, created?.Name);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenInvalidModel()
        {
            // missing password
            var dto = new UserRequestDTO
            {
                Email = "bad@",
                Name = "",
                Password = ""
            };

            var res = await _client.PostAsJsonAsync("/api/users/register", dto);
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        public async Task Register_ReturnsConflict_WhenDuplicate()
        {
            // first registration
            var dto = new UserRequestDTO
            {
                Email = "dup@user.com",
                Name = "dupuser",
                Password = "Password1!"
            };
            var first = await _client.PostAsJsonAsync("/api/users/register", dto);
            first.EnsureSuccessStatusCode();

            // duplicate attempt
            var res2 = await _client.PostAsJsonAsync("/api/users/register", dto);
            Assert.Equal(HttpStatusCode.Conflict, res2.StatusCode);
        }

        #endregion

        #region Login

        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsValid()
        {
            // ensure seeded user exists
            var login = new LoginRequestDTO
            {
                Email = "seed@test.com",
                Password = "Test123!"
            };

            await SeedAndLogin(login);

            // if no exception, login succeeded and header set
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenMissingFields()
        {
            var dto = new LoginRequestDTO { Email = "", Password = "" };
            var res = await _client.PostAsJsonAsync("/api/users/login", dto);
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        public async Task Login_ReturnsForbidden_WhenInvalidCredentials()
        {
            var dto = new LoginRequestDTO
            {
                Email = "noone@nowhere.com",
                Password = "wrong"
            };
            var res = await _client.PostAsJsonAsync("/api/users/login", dto);
            Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
        }

        #endregion

        #region Logout

        [Fact]
        public async Task Logout_ReturnsNoContent_WhenAuthenticated()
        {
            var login = new LoginRequestDTO
            {
                Email = "seed@test.com",
                Password = "Test123!"
            };
            await SeedAndLogin(login);

            // now logout
            var res = await _client.PostAsync("/api/users/logout", null);
            Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
        }

        [Fact]
        public async Task Logout_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var res = await _client.PostAsync("/api/users/logout", null);
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        #endregion

        #region Helpers

        private async Task SeedAndLogin(LoginRequestDTO login)
        {
            // create the user if not already present
            using var scope = _factory.Services.CreateScope();
            var uMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var existing = await uMgr.FindByEmailAsync(login.Email);
            if (existing == null)
            {
                var user = new ApplicationUser
                {
                    Email = login.Email,
                    UserName = login.Email
                };
                await uMgr.CreateAsync(user, login.Password);
            }

            // perform login
            var res = await _client.PostAsJsonAsync("/api/users/login", login);
            res.EnsureSuccessStatusCode();

            var dto = await res.Content.ReadFromJsonAsync<LoginResponseDTO>();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", dto!.AuthToken);
        }

        private static async Task SeedUser(UserManager<ApplicationUser> uMgr)
        {
            var email = "seed@test.com";
            var pwd = "Test123!";
            if (await uMgr.FindByEmailAsync(email) == null)
            {
                var user = new ApplicationUser
                {
                    Email = email,
                    UserName = email
                };
                var result = await uMgr.CreateAsync(user, pwd);
                if (!result.Succeeded)
                    throw new InvalidOperationException("Seeding user failed");
            }
        }

        public void Dispose()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<VotingSystemDbContext>();
            db.Database.EnsureDeleted();
        }

        #endregion
    }

    
}
