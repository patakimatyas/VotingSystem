using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VotingSystem.DataAccess.Models;
using VotingSystem.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using VotingSystem.DataAccess;
using Moq;
using Xunit;

namespace VotingSystem.Tests.UnitTests
{
    public class PollServiceTest : IDisposable
    {
        private readonly VotingSystemDbContext _context;
        private readonly IPollService _pollService;

        public PollServiceTest()
        {
            var options = new DbContextOptionsBuilder<VotingSystemDbContext>()
                .UseInMemoryDatabase(databaseName: "VotingSystemTestDb")
                .Options;

            _context = new VotingSystemDbContext(options);
            _context.Database.EnsureCreated();

            _pollService = new PollService(_context);
            SeedDatabase();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SeedDatabase()
        {
            if (_context.Polls.Any()) return;

            var now = DateTime.UtcNow;

            // 1. Active poll
            _context.Polls.Add(new Poll
            {
                Question = "Active Poll",
                StartDate = now.AddDays(-1),
                EndDate = now.AddDays(1),
                CreatedByUserId = "user1",
                Options = new List<Option>
        {
            new Option { Text = "Yes" },
            new Option { Text = "No" }
        },
                Votes = new List<Vote>
        {
            new Vote { Option = new Option { Text = "Yes" }, PollId = 0 } // one dummy vote
        }
            });

            // 2. Closed poll
            _context.Polls.Add(new Poll
            {
                Question = "Closed Poll",
                StartDate = now.AddDays(-5),
                EndDate = now.AddDays(-1),
                CreatedByUserId = "user2",
                Options = new List<Option>
        {
            new Option { Text = "A" },
            new Option { Text = "B" }
        }
            });

            // 3. Future poll
            _context.Polls.Add(new Poll
            {
                Question = "Future Poll",
                StartDate = now.AddDays(1),
                EndDate = now.AddDays(3),
                CreatedByUserId = "user1",
                Options = new List<Option>
        {
            new Option { Text = "X" },
            new Option { Text = "Y" }
        }
            });

            // 4. Closed poll with keyword
            _context.Polls.Add(new Poll
            {
                Question = "FilterTest Poll",
                StartDate = now.AddDays(-4),
                EndDate = now.AddDays(-2),
                CreatedByUserId = "user3",
                Options = new List<Option>
        {
            new Option { Text = "Foo" },
            new Option { Text = "Bar" }
        }
            });

            _context.SaveChanges();
        }


        #region Add
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreatePollAsync_ThrowsArgumentException_WhenQuestionIsNullOrWhitespace(string? q)
        {
            // arrange
            var p = new Poll
            {
                Question = q!,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1),
                Options = new List<Option>
            {
                new Option { Text = "One" },
                new Option { Text = "Two" }
            }
            };

            // act / assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _pollService.CreatePollAsync(p));
            Assert.Contains("Poll question cannot be empty", ex.Message);
        }

        [Fact]
        public async Task CreatePollAsync_ThrowsArgumentException_WhenOptionsNullOrTooFew()
        {
            var basePoll = new Poll
            {
                Question = "Q?",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1)
            };

            // options null
            var ex1 = await Assert.ThrowsAsync<ArgumentException>(
                () => _pollService.CreatePollAsync(basePoll));
            Assert.Contains("A poll must have at least 2 options", ex1.Message);

            // options count < 2
            basePoll.Options = new List<Option> { new Option { Text = "OnlyOne" } };
            var ex2 = await Assert.ThrowsAsync<ArgumentException>(
                () => _pollService.CreatePollAsync(basePoll));
            Assert.Contains("A poll must have at least 2 options", ex2.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreatePollAsync_ThrowsArgumentException_WhenAnyOptionTextIsNullOrWhitespace(string? badText)
        {
            var p = new Poll
            {
                Question = "Valid?",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1),
                Options = new List<Option>
            {
                new Option { Text = "OK" },
                new Option { Text = badText! }
            }
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _pollService.CreatePollAsync(p));
            Assert.Contains("Poll options must have text", ex.Message);
        }

        [Fact]
        public async Task CreatePollAsync_ThrowsArgumentException_WhenStartDateAfterEndDate()
        {
            var p = new Poll
            {
                Question = "Q",
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(1),
                Options = new List<Option>
            {
                new Option { Text = "One" },
                new Option { Text = "Two" }
            }
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _pollService.CreatePollAsync(p));
            Assert.Contains("Start date cannot be after end date", ex.Message);
        }
        [Fact]
        public async Task CreatePollAsync_AddsPoll_WhenAllDataValid()
        {
            // arrange
            var initialCount = _context.Polls.Count();
            var now = DateTime.UtcNow;
            var p = new Poll
            {
                Question = "Új kérdés?",
                StartDate = now,
                EndDate = now.AddDays(2),
                Options = new List<Option>
            {
                new Option { Text = "Igen" },
                new Option { Text = "Nem" }
            }
            };

            // act
            await _pollService.CreatePollAsync(p);

            // assert
            var after = _context.Polls.Count();
            Assert.Equal(initialCount + 1, after);

            var added = _context.Polls.SingleOrDefault(x => x.Question == "Új kérdés?");
            Assert.NotNull(added);
            Assert.Equal(2, added.Options.Count);
        }
        #endregion

        #region Get

        [Fact]
        public async Task GetAllPollsAsync_ReturnsEveryPollWithOptions()
        {
            var all = await _pollService.GetAllPollsAsync();
            // We seeded 4 polls above
            Assert.Equal(4, all.Count);
            Assert.All(all, p => Assert.NotEmpty(p.Options));
        }

        [Fact]
        public async Task GetActivePollsAsync_ReturnsOnlyCurrentlyActive()
        {
            var active = await _pollService.GetActivePollsAsync();

            // Only the one whose date-range straddles now
            Assert.Single(active);
            Assert.Equal("Active Poll", active[0].Question);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenIdNotFound()
        {
            var none = await _pollService.GetByIdAsync(9999);
            Assert.Null(none);
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsArgumentOutOfRange_WhenIdNonPositive()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _pollService.GetByIdAsync(0));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsPollWithOptionsAndVotes()
        {
            // Grab the active one we seeded (its Id will be 1)
            var p = (await _pollService.GetActivePollsAsync()).First();
            var byId = await _pollService.GetByIdAsync(p.Id);

            Assert.NotNull(byId);
            Assert.Equal(p.Question, byId!.Question);
            Assert.NotEmpty(byId.Options);
            Assert.NotEmpty(byId.Votes);
        }


        [Fact]
        public async Task GetClosedPollsAsync_ReturnsOnlyEndedPolls()
        {
            var closed = await _pollService.GetClosedPollsAsync(null, null, null);

            // We seeded two polls whose EndDate < now: "Closed Poll" and "FilterTest Poll"
            Assert.Equal(2, closed.Count);
            Assert.DoesNotContain(closed, p => p.Question == "Active Poll");
        }

        [Fact]
        public async Task GetClosedPollsAsync_FiltersByText()
        {
            var filtered = await _pollService.GetClosedPollsAsync("FilterTest", null, null);
            Assert.Single(filtered);
            Assert.Equal("FilterTest Poll", filtered[0].Question);
        }
        [Fact]
        public async Task GetPollsByUserAsync_ThrowsArgumentException_WhenUserIdEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _pollService.GetPollsByUserAsync("   "));
        }

        [Fact]
        public async Task GetPollsByUserAsync_ReturnsOnlyThatUsersPolls()
        {
            var u1 = await _pollService.GetPollsByUserAsync("user1");
            // We gave user1 two polls: Active Poll and Future Poll
            Assert.Equal(2, u1.Count);
            Assert.All(u1, p => Assert.Equal("user1", p.CreatedByUserId));
        }


        #endregion
    }
}
