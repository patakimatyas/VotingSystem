using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using VotingSystem.DataAccess;
using VotingSystem.DataAccess.Models;
using VotingSystem.Shared.DTOs;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;

namespace VotingSystem.Tests.IntegrationTests
{
    public class PollsControllerIntegrationTests
        : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly string _dbName = Guid.NewGuid().ToString();

        // credentials for our seeded user
        private static readonly LoginRequestDTO UserLogin = new()
        {
            Email = "teszt@gmail.com",
            Password = "Teszt123!"
        };

        public PollsControllerIntegrationTests()
        {
            Environment.SetEnvironmentVariable(
                "ASPNETCORE_ENVIRONMENT", "IntegrationTest");

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("IntegrationTest");
                    builder.ConfigureServices(services =>
                    {
                        // 1) replace real DB with in-memory
                        var desc = services.SingleOrDefault(
                            d => d.ServiceType == typeof(
                                     DbContextOptions<VotingSystemDbContext>));
                        if (desc != null) services.Remove(desc);

                        services.AddDbContext<VotingSystemDbContext>(opts =>
                            opts.UseInMemoryDatabase(_dbName));

                        // 2) seed polls and a test user
                        using var scope = services
                            .BuildServiceProvider()
                            .CreateScope();
                        var sp = scope.ServiceProvider;
                        var db = sp.GetRequiredService<VotingSystemDbContext>();
                        var uMgr = sp.GetRequiredService<
                                       Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();

                        db.Database.EnsureCreated();
                        SeedPolls(db);
                        SeedUser(uMgr).Wait();
                    });
                });

            _client = _factory.CreateClient();
        }

        #region ClosedPollsList

        [Fact]
        public async Task GetClosedPolls_Unauthorized_WhenNoJwt()
        {
            var res = await _client.GetAsync("/api/polls/closed");
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        [Fact]
        public async Task GetClosedPolls_ReturnsOnlyClosed_WhenAuthed()
        {
            await Login(UserLogin);

            var res = await _client.GetAsync("/api/polls/closed");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);

            var list = await res.Content.ReadFromJsonAsync<List<PollResponseDTO>>();
            Assert.Single(list!);
            Assert.Equal(2, list![0].Id);    // csak a zárt poll #2
        }

        #endregion

        #region ClosedPollResult

        [Fact]
        public async Task GetClosedPollResult_ReturnsNotFound_WhenOpenOrMissing()
        {
            await Login(UserLogin);
            // open poll (1) → 404
            var r1 = await _client.GetAsync("/api/polls/closed/1");
            Assert.Equal(HttpStatusCode.NotFound, r1.StatusCode);

            // nem létező → 404
            var r2 = await _client.GetAsync("/api/polls/closed/999");
            Assert.Equal(HttpStatusCode.NotFound, r2.StatusCode);
        }

        [Fact]
        public async Task GetClosedPollResult_ReturnsOk_WhenClosed()
        {
            await Login(UserLogin);

            var res = await _client.GetAsync("/api/polls/closed/2");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);

            var dto = await res.Content.ReadFromJsonAsync<ClosedPollResponseDTO>();
            Assert.NotNull(dto);
            Assert.Equal(2, dto!.Id);
            Assert.All(dto.Options, o =>
                Assert.InRange(o.VotePercentage, 0.0, 100.0));
        }

        #endregion

        #region GetPollById
        [Fact]
        public async Task GetPollById_Unauthorized_WhenNoJwt()
        {
            // Arrange: no Authorization header

            // Act
            var response = await _client.GetAsync("/api/polls/1");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        [Fact]
        public async Task GetPollById_ReturnsOk_WhenExists()
        {
            await Login(UserLogin);

            var res = await _client.GetAsync("/api/polls/1");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);

            var dto = await res.Content
                .ReadFromJsonAsync<PollResponseDTO>();
            Assert.NotNull(dto);
            Assert.Equal(1, dto!.Id);
            Assert.Equal("Favorite color?", dto.Question);
        }
        [Fact]
        public async Task GetPollById_ReturnsNotFound_WhenMissing()
        {
            await Login(UserLogin);

            var res = await _client.GetAsync("/api/polls/999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);

            var problem = await res.Content
                .ReadFromJsonAsync<ProblemDetails>();
            Assert.NotNull(problem);
        }
        #endregion

        #region GetActivePolls

        [Fact]
        public async Task GetActivePolls_Unauthorized_WhenNoJwt()
        {
            // Arrange: no Authorization header

            // Act
            var response = await _client.GetAsync("/api/polls/active");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        [Fact]
        public async Task GetActivePolls_ReturnsOnlyOpen()
        {
            // Act
            var res = await _client.GetAsync("/api/polls/active");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);

            // Login first
            await Login(UserLogin);

            res = await _client.GetAsync("/api/polls/active");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);

            var list = await res.Content
                .ReadFromJsonAsync<List<PollResponseDTO>>();
            Assert.Single(list!);
            Assert.Equal(1, list![0].Id);
        }
        #endregion

        #region CreatePoll
        [Fact]
        public async Task CreatePoll_ReturnsBadRequest_WhenInvalidModel()
        {
            await Login(UserLogin);

            // Csak 1 opció → szolgáltatás dob ArgumentException-t, amit controller 400-ra fordít
            var dto = new PollRequestDTO
            {
                Question = "Test?",
                Options = new List<string> { "X" }
            };

            var res = await _client.PostAsJsonAsync("/api/polls/create", dto);
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
            
        }

        [Fact]
        public async Task CreatePoll_Unauthorized_Then_Succeeds()
        {
            var dto = new PollRequestDTO
            {
                Question = "New?",
                Options = new List<string> { "A", "B" }
            };

            // Without JWT
            var res = await _client.PostAsJsonAsync(
                "/api/polls/create", dto);
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);

            // With JWT
            await Login(UserLogin);

            res = await _client.PostAsJsonAsync(
                "/api/polls/create", dto);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);

            // Now ensure it appears in /api/polls/mine
            res = await _client.GetAsync("/api/polls/mine");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var mine = await res.Content
                .ReadFromJsonAsync<List<PollResponseDTO>>();
            Assert.Contains(mine!, p => p.Question == "New?");
        }
        #endregion

        #region Helpers
        private async Task Login(LoginRequestDTO loginRequest)
        {
            var response = await _client.PostAsJsonAsync("/api/users/login", loginRequest);
            var text = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"LOGIN FAILED: {response.StatusCode} / {text}");
            response.EnsureSuccessStatusCode();

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse?.AuthToken);
        }

        private static void SeedPolls(VotingSystemDbContext db)
        {
            var now = DateTime.Now;
            var p1 = new Poll
            {
                Id = 1,
                Question = "Favorite color?",
                StartDate = now.AddDays(-1),
                EndDate = now.AddDays(+1),
                CreatedByUserId = "teszt",
                Options = new List<Option>
                {
                    new() { Id = 10, Text = "Red"  },
                    new() { Id = 11, Text = "Blue" }
                }
            };
            var p2 = new Poll
            {
                Id = 2,
                Question = "Tea or coffee?",
                StartDate = now.AddDays(-2),
                EndDate = now.AddDays(-1),
                IsClosed = true,
                CreatedByUserId = "user2",
                Options = new List<Option>
                {
                    new() { Id = 20, Text = "Tea"    },
                    new() { Id = 21, Text = "Coffee" }
                }
            };

            db.Polls.AddRange(p1, p2);
            db.SaveChanges();
        }

        private async static Task SeedUser(UserManager<ApplicationUser> uMgr)
        {
            var user = uMgr.FindByEmailAsync(UserLogin.Email).Result;
            if (user == null)
            {
                user = new ApplicationUser { UserName = UserLogin.Email, Email = UserLogin.Email, Name = "Test User" };
                var result = await uMgr.CreateAsync(user, UserLogin.Password);
                if (!result.Succeeded)
                    throw new InvalidOperationException($"Seeding test user failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        public void Dispose()
        {
            // drop the in-memory DB
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider
                     .GetRequiredService<VotingSystemDbContext>();
            db.Database.EnsureDeleted();
        }
        #endregion
    }
}
