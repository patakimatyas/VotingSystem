using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using VotingSystem.DataAccess;
using VotingSystem.DataAccess.Models;
using VotingSystem.DataAccess.Services;
using VotingSystem.Shared.DTOs;
using VotingSystem.WebAPI.Controllers;
using VotingSystem.WebAPI.Infrastructure;
using Xunit;

namespace VotingSystem.Tests.ControllerTests
{
    public class PollsControllerTest : IDisposable
    {
        private readonly VotingSystemDbContext _context;
        private readonly PollsController _controller;
        private readonly Mock<IUserService> _mockUserService;

        public PollsControllerTest()
        {
            var options = new DbContextOptionsBuilder<VotingSystemDbContext>()
           .UseInMemoryDatabase(Guid.NewGuid().ToString())
           .Options;
            _context = new VotingSystemDbContext(options);

            var pollService = new PollService(_context);

            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<WebApiMappingProfile>());
            var mapper = mapperConfig.CreateMapper();
            mapperConfig.AssertConfigurationIsValid();

            _mockUserService = new Mock<IUserService>();

            // by default simulate authenticated user “user1”
            _mockUserService
                .Setup(u => u.GetCurrentUserId())
                .Returns("user1");
            // so that owner-only branches have something to work with
            _mockUserService
                .Setup(u => u.GetAllAsync())
                .ReturnsAsync(new List<ApplicationUser>
                {
                new() { Id = "user1", Email = "u1@example.com" },
                new() { Id = "user2", Email = "u2@example.com" }
                });

            _controller = new PollsController(
           pollService,
           mapper,
           _mockUserService.Object
            );

            SeedDatabase();

        }
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SeedDatabase()
        {
            // open poll #1 (created by user1)
            var p1 = new Poll
            {
                Id = 1,
                Question = "Favorite color?",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(+1),
                CreatedByUserId = "user1",
                Options = new List<Option>
            {
                new() { Id = 10, Text = "Red"  },
                new() { Id = 11, Text = "Blue" }
            }
            };
            // add one vote
            p1.Votes.Add(new Vote { PollId = 1, OptionId = 11});

            // closed poll #2 (created by user2)
            var p2 = new Poll
            {
                Id = 2,
                Question = "Tea or coffee?",
                StartDate = DateTime.UtcNow.AddDays(-2),
                EndDate = DateTime.UtcNow.AddDays(-1),
                CreatedByUserId = "user2",
                IsClosed = true,
                Options = new List<Option>
            {
                new() { Id = 20, Text = "Tea"    },
                new() { Id = 21, Text = "Coffee" }
            }
            };

            _context.Polls.AddRange(p1, p2);
            _context.SaveChanges();

            _context.Voters.Add(new Voter { UserId = "user2", PollId = 1 });
            _context.SaveChanges();
        }

        #region Get

        [Fact]
        public async Task GetPollById_NotFound_WhenMissing()
        {
            // Arrange: authenticated, but poll 999 does not exist
            _mockUserService.Setup(u => u.GetCurrentUserId()).Returns("user1");

            // Act
            var actionResult = await _controller.GetPollById(999);

            // Assert
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task GetPollById_ReturnsOk_WithCorrectDto_ForOwner()
        {
            // Arrange: authenticated as the owner of poll #1
            _mockUserService.Setup(u => u.GetCurrentUserId())
                            .Returns("user1");

            // Act
            var actionResult = await _controller.GetPollById(1);

            // Assert: 200 OK
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var dto = Assert.IsType<PollResponseDTO>(okResult.Value);

            // Basic poll fields
            Assert.Equal(1, dto.Id);
            Assert.Equal("Favorite color?", dto.Question);
            Assert.Equal(2, dto.Options.Count);

            // Owner hasn't voted on their own poll
            Assert.False(dto.HasVoted);

            // Because we're the owner, we should see **all** users
            // (we seeded two users: "user1" and "user2").
            Assert.Equal(2, dto.Voters.Count);

            // Find each voter
            var me = dto.Voters.Single(v => v.UserId == "user1");
            var other = dto.Voters.Single(v => v.UserId == "user2");

            // Owner (user1) has not voted
            Assert.False(me.HasVoted);

            // Other user (user2) did vote option 11
            Assert.True(other.HasVoted);
        }

        [Fact]
        public async Task GetActivePolls_ReturnsOnlyCurrentlyActive_WithHasVotedFlag()
        {
            // Arrange: default user1 (has not voted on poll #1)
            _mockUserService.Setup(u => u.GetCurrentUserId()).Returns("user1");

            // Act
            var actionResult = await _controller.GetActivePolls();

            // Assert: 200 OK
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

            // Payload is list of PollResponseDTO
            var list = Assert.IsType<List<PollResponseDTO>>(okResult.Value);

            // We seeded 1 open poll (#1) and 1 closed poll (#2)
            Assert.Single(list);

            var dto = list[0];
            Assert.Equal(1, dto.Id);
            Assert.Equal("Favorite color?", dto.Question);

            // user1 did not vote in poll #1
            Assert.False(dto.HasVoted);
        }

        [Fact]
        public async Task GetClosedPolls_ReturnsOnlyClosedPolls()
        {
            // Arrange: authenticated as default “user1”
            _mockUserService.Setup(u => u.GetCurrentUserId()).Returns("user1");

            // Act: no filters
            var actionResult = await _controller.GetClosedPolls(null, null, null);

            // Assert: 200 OK
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

            // Payload is List<PollResponseDTO>
            var list = Assert.IsType<List<PollResponseDTO>>(okResult.Value);

            // We seeded exactly one closed poll (ID=2) in SeedDatabase()
            Assert.Single(list);
            Assert.Equal(2, list[0].Id);
            Assert.Equal("Tea or coffee?", list[0].Question);
        }

        [Fact]
        public async Task GetClosedPollResult_NotFound_WhenStillOpenOrMissing()
        {
            // Arrange: authenticated as “user1” by default
            _mockUserService.Setup(u => u.GetCurrentUserId()).Returns("user1");

            // Act & Assert #1: poll #1 is still open → 404
            var res1 = await _controller.GetClosedPollResult(1);
            Assert.IsType<NotFoundResult>(res1.Result);

            // Act & Assert #2: poll 999 does not exist → 404
            var res2 = await _controller.GetClosedPollResult(999);
            Assert.IsType<NotFoundResult>(res2.Result);
        }

        [Fact]
        public async Task GetClosedPollResult_ReturnsDto_WhenClosed()
        {
            // Arrange
            _mockUserService.Setup(u => u.GetCurrentUserId()).Returns("user1");

            // Act
            var actionResult = await _controller.GetClosedPollResult(2);

            // Assert 200 OK
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var dto = Assert.IsType<ClosedPollResponseDTO>(ok.Value);

            // closed‐poll ID matches seed
            Assert.Equal(2, dto.Id);

            // each option’s percentage is between 0 and 100
            Assert.All(dto.Options, opt =>
                Assert.InRange(opt.VotePercentage, 0.0, 100.0));
        }

        [Fact]
        public async Task GetMyPolls_ReturnsOnlyOwned()
        {
            // Arrange: user1 owns poll #1
            _mockUserService.Setup(u => u.GetCurrentUserId()).Returns("user1");

            // Act
            var actionResult = await _controller.GetMyPolls();

            // Assert 200 OK
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var list = Assert.IsType<List<PollResponseDTO>>(ok.Value);

            // Only the seed‐poll #1 should appear
            Assert.Single(list);
            Assert.Equal(1, list[0].Id);
        }


        #endregion

        #region Post
       

        [Fact]
        public async Task CreatePoll_BadRequest_WhenModelStateInvalid()
        {
            // Arrange: simulate an authenticated user
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "test"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var dto = new PollRequestDTO
            {
                Question = "Ignored because ModelState is bad",
                Options = new List<string> { "A", "B" }
            };

            // Force ModelState to be invalid
            _controller.ModelState.AddModelError("Question", "Required");

            // Act
            var result = await _controller.CreatePoll(dto);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(bad.Value);
        }

        [Fact]
        public async Task CreatePoll_BadRequest_WhenServiceThrowsArgumentException()
        {
            // Arrange: authenticated as user1
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "test"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // DTO with only one option → service will throw ArgumentException
            var dto = new PollRequestDTO
            {
                Question = "Too few options",
                Options = new List<string> { "OnlyOne" }
            };

            // Act
            var result = await _controller.CreatePoll(dto);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("at least 2 options", bad.Value?.ToString());
        }

        [Fact]
        public async Task CreatePoll_Succeeds_ReturnsOk_AndPersists()
        {
            // Arrange: authenticated as user1
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "test"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var dto = new PollRequestDTO
            {
                Question = "What’s 2+2?",
                Options = new List<string> { "3", "4", "5" }
            };

            // Act
            var result = await _controller.CreatePoll(dto);

            // Assert: 200 OK
            Assert.IsType<OkResult>(result);

            // The poll should now exist in the DB
            var saved = _context.Polls
                .Include(p => p.Options)
                .Single(p => p.Question == "What’s 2+2?");

            Assert.Equal("user1", saved.CreatedByUserId);
            Assert.False(saved.IsClosed);
            Assert.Equal(3, saved.Options.Count);
        }
        #endregion
    }

}
