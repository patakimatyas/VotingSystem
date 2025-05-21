using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;
using VotingSystem.WebAPI;
using VotingSystem.Shared.DTOs;
using VotingSystem.DataAccess;
using VotingSystem.DataAccess.Models;
using Microsoft.AspNetCore.Identity;

namespace VotingSystem.Tests.IntegrationTests
{
    public class VotesControllerIntegrationTests
        : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        // we will seed this user and login as them
        private const string TestUserEmail = "voter@example.com";
        private const string TestUserPassword = "Voter@123";

        public VotesControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            // force in-memory and IntegrationTest env
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "IntegrationTest");

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // replace the real DB with in-memory
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<VotingSystemDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<VotingSystemDbContext>(opts =>
                        opts.UseInMemoryDatabase("TestVotesDB"));

                    // build the provider to seed
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<VotingSystemDbContext>();
                    db.Database.EnsureCreated();

                    SeedPolls(db);
                    SeedUser(scope.ServiceProvider).GetAwaiter().GetResult();
                });
            });
            _client = _factory.CreateClient();
        }

        public void Dispose()
        {
            // clean up in‐memory db
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<VotingSystemDbContext>();
            db.Database.EnsureDeleted();
        }
      
        
        private async Task<string> LoginAndGetJwtAsync()
        {
            var login = new LoginRequestDTO
            {
                Email = TestUserEmail,
                Password = TestUserPassword
            };
            var res = await _client.PostAsJsonAsync("/api/users/Login", login);
            res.EnsureSuccessStatusCode();
            var dto = await res.Content.ReadFromJsonAsync<LoginResponseDTO>();
            return dto!.AuthToken;
        }

        [Fact]
        public async Task Vote_Unauthorized_WhenNoJwt()
        {
            var vote = new VoteRequestDTO { PollId = 1, OptionId = 10 };
            var res = await _client.PostAsJsonAsync("/api/votes", vote);
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        [Fact]
        public async Task Vote_BadRequest_WhenPollNotFound()
        {
            var jwt = await LoginAndGetJwtAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

            var vote = new VoteRequestDTO { PollId = 999, OptionId = 10 };
            var res = await _client.PostAsJsonAsync("/api/votes", vote);
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        public async Task Vote_BadRequest_WhenOptionInvalid()
        {
            var jwt = await LoginAndGetJwtAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

            // Poll #1 exists, options are 10 & 11—use a bogus one:
            var vote = new VoteRequestDTO { PollId = 1, OptionId = 999 };
            var res = await _client.PostAsJsonAsync("/api/votes", vote);
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        public async Task Vote_BadRequest_WhenPollClosed()
        {
            var jwt = await LoginAndGetJwtAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

            // Poll #2 is seeded as closed
            var vote = new VoteRequestDTO { PollId = 2, OptionId = 20 };
            var res = await _client.PostAsJsonAsync("/api/votes", vote);
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        public async Task Vote_NoContent_WhenSuccess()
        {
            var jwt = await LoginAndGetJwtAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

            var vote = new VoteRequestDTO { PollId = 1, OptionId = 10 };
            var res = await _client.PostAsJsonAsync("/api/votes", vote);
            Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
        }

        private static void SeedPolls(VotingSystemDbContext db)
        {
            var now = DateTime.UtcNow;

            // Open poll
            db.Polls.Add(new Poll
            {
                Id = 1,
                Question = "Open poll?",
                StartDate = now.AddDays(-1),
                EndDate = now.AddDays(+1),
                CreatedByUserId = "unused",
                Options = new List<Option>
                {
                    new() { Id = 10, Text = "Yes" },
                    new() { Id = 11, Text = "No" }
                }
            });

            // Closed poll
            db.Polls.Add(new Poll
            {
                Id = 2,
                Question = "Closed poll?",
                StartDate = now.AddDays(-2),
                EndDate = now.AddDays(-1),
                IsClosed = true,
                CreatedByUserId = "unused",
                Options = new List<Option>
                {
                    new() { Id = 20, Text = "A" },
                    new() { Id = 21, Text = "B" }
                }
            });

            db.SaveChanges();
        }

        private static async Task SeedUser(IServiceProvider sp)
        {
            var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                UserName = TestUserEmail,
                Email = TestUserEmail
            };
            await userMgr.CreateAsync(user, TestUserPassword);
        }
    }
}
