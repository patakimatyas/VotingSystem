using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VotingSystem.Shared.DTOs;
using VotingSystem.DataAccess.Services;
using VotingSystem.WebAPI.Controllers;
using Xunit;

namespace VotingSystem.Tests.ControllerTests
{
    public class VotesControllerTest : IDisposable
    {
        private readonly Mock<IVoteService> _mockVoteService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly VotesController _controller;

        public VotesControllerTest()
        {
            _mockVoteService = new Mock<IVoteService>();
            _mockUserService = new Mock<IUserService>();

            // We don't actually use the IMapper in Vote(), so just pass a dummy
            var dummyMapper = new Mock<AutoMapper.IMapper>().Object;

            _controller = new VotesController(
                _mockVoteService.Object,
                _mockUserService.Object,
                dummyMapper
            );

            // give the controller a stubbed HttpContext (required by Authorize filter)
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }
        public void Dispose()
        {
        }

        [Fact]
        public async Task Vote_BadRequest_WhenServiceThrowsArgumentException()
        {
            // Arrange: simulate an authenticated user
            _mockUserService
                .Setup(u => u.GetCurrentUserId())
                .Returns("user42");

            // service will reject this vote
            _mockVoteService
                .Setup(v => v.VoteAsync(1, 10, "user42"))   
                .ThrowsAsync(new ArgumentException("Invalid option"))
                .Verifiable();

            var dto = new VoteRequestDTO { PollId = 1, OptionId = 10 };

            // Act
            var result = await _controller.Vote(dto);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            // we wrap the message in an anonymous { message = ex.Message }
            Assert.Contains("Invalid option", bad.Value!.ToString());

            _mockVoteService.Verify();
        }

        [Fact]
        public async Task Vote_Success_ReturnsNoContent()
        {
            // Arrange
            _mockUserService
                .Setup(u => u.GetCurrentUserId())
                .Returns("user42");

            _mockVoteService
                .Setup(v => v.VoteAsync(5, 55, "user42"))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var dto = new VoteRequestDTO { PollId = 5, OptionId = 55 };

            // Act
            var result = await _controller.Vote(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockVoteService.Verify();
        }
    }
}
