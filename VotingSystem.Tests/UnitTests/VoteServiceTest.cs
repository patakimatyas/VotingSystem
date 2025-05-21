using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VotingSystem.DataAccess.Models;
using VotingSystem.DataAccess;
using VotingSystem.DataAccess.Services;
using Xunit;

namespace VotingSystem.Tests.UnitTests
{
    public class VoteServiceTest : IDisposable
    {
        private readonly VotingSystemDbContext _context;
        private readonly VoteService _voteService;

        public VoteServiceTest()
        {
            var opts = new DbContextOptionsBuilder<VotingSystemDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new VotingSystemDbContext(opts);
            _context.Database.EnsureCreated();

            _voteService = new VoteService(_context);
            

            SeedDataBase();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
        private void SeedDataBase()
        {
            var openPoll = new Poll
            {
                Id = 1,
                Question = "Open?",
                StartDate = DateTime.UtcNow.AddHours(-1),
                EndDate = DateTime.UtcNow.AddHours(+1),
                Options =
                {
                    new Option { Id = 10, Text = "Yes" },
                    new Option { Id = 11, Text = "No" }
                }
            };
            var closedPoll = new Poll
            {
                Id = 2,
                Question = "Closed?",
                StartDate = DateTime.UtcNow.AddDays(-2),
                EndDate = DateTime.UtcNow.AddDays(-1),
                Options =
                {
                    new Option { Id = 20, Text = "Option" }
                },
                IsClosed = true
            };

            _context.Polls.AddRange(openPoll, closedPoll);
            _context.SaveChanges();
        }

        #region Vote

        [Fact]
        public async Task VoteAsync_PollDoesNotExist_ThrowsArgumentException()
        {
            // pollId 999 does not exist at all
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _voteService.VoteAsync(999, optionId: 10, userId:"user"));
        }

        [Fact]
        public async Task VoteAsync_PollIsClosed_ThrowsArgumentException()
        {
            // pollId 2 is seeded with IsClosed = true
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _voteService.VoteAsync(2, optionId: 20, userId: "user"));
        }

        [Fact]
        public async Task VoteAsync_OptionNotInPoll_ThrowsArgumentException()
        {
            // pollId 1 is open, but optionId 999 isn’t one of its options
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _voteService.VoteAsync(1, optionId: 999, userId: "user"));
        }

        [Fact]
        public async Task VoteAsync_UserAlreadyVoted_ThrowsInvalidOperationException()
        {
            // first vote succeeds
            await _voteService.VoteAsync(1, optionId: 10, userId: "user");

            // second attempt by same user in same poll must fail
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _voteService.VoteAsync(1, optionId: 11, userId: "user"));
        }

        [Fact]
        public async Task VoteAsync_ValidVote_AddsVoteToDatabase()
        {
            // act
            await _voteService.VoteAsync(1, optionId: 11, userId: "user");

            // assert: exactly one vote row with those keys
            var vote = _context.Votes.Single();
            Assert.Equal(1, vote.PollId);
            Assert.Equal(11, vote.OptionId);
        }
        #endregion
    }
}
