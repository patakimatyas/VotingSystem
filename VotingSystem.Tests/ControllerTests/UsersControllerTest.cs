using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VotingSystem.DataAccess.Models;
using VotingSystem.DataAccess.Services;
using VotingSystem.Shared.DTOs;
using VotingSystem.WebAPI.Controllers;
using Xunit;

namespace VotingSystem.Tests.ControllerTests
{
    public class UsersControllerTest
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly IMapper _mapper;
        private readonly UsersController _controller;

        public UsersControllerTest()
        {
            // 1) Mock the IUserService
            _mockUserService = new Mock<IUserService>();

            // 2) Configure AutoMapper exactly as in your WebMappingProfile:
            var config = new MapperConfiguration(cfg =>
            {
                // map incoming DTO → ApplicationUser
                cfg.CreateMap<UserRequestDTO, ApplicationUser>()
                    .ForSourceMember(src => src.Password, opt => opt.DoNotValidate())
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

                // map ApplicationUser → outgoing DTO
                cfg.CreateMap<ApplicationUser, UserResponseDTO>();
            });
            _mapper = config.CreateMapper();

            // 3) Instantiate controller under test
            _controller = new UsersController(_mockUserService.Object, _mapper);
        }

        #region Register
        [Fact]
        public async Task Register_ReturnsBadRequest_OnArgumentException()
        {
            var dto = new UserRequestDTO
            {
                Email = "bad@x.com",
                Name = "Bad",
                Password = "pw"
            };

            _mockUserService
                .Setup(s => s.RegisterUserAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ThrowsAsync(new ArgumentException("Invalid input"))
                .Verifiable();

            var result = await _controller.Register(dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid input", bad.Value);
            _mockUserService.Verify();
        }

        [Fact]
        public async Task Register_ReturnsConflict_OnInvalidOperationException()
        {
            var dto = new UserRequestDTO
            {
                Email = "dup@x.com",
                Name = "Dup",
                Password = "pw"
            };

            _mockUserService
                .Setup(s => s.RegisterUserAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ThrowsAsync(new InvalidOperationException("Already exists"))
                .Verifiable();

            var result = await _controller.Register(dto);

            var cf = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("Already exists", cf.Value);
            _mockUserService.Verify();
        }
        [Fact]
        public async Task Register_ReturnsCreated_WithUserResponseDto()
        {
            // Arrange
            var dto = new UserRequestDTO
            {
                Email = "alice@x.com",
                Name = "Alice",
                Password = "Secure!23"
            };
            // we expect service to succeed without throwing
            _mockUserService
                .Setup(s => s.RegisterUserAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _controller.Register(dto);

            // Assert
            var created = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, created.StatusCode);

            var response = Assert.IsType<UserResponseDTO>(created.Value);
            Assert.Equal(dto.Email, response.Email);
            Assert.Equal(dto.Name, response.Name);

            _mockUserService.Verify();
        }
        #endregion

        #region Login
        [Fact] 
        public async Task Login_ReturnsBadRequest_OnArgumentException()
        {
            var login = new LoginRequestDTO
            {
                Email = "bad@x.com",
                Password = "pw"
            };

            _mockUserService
                .Setup(s => s.LoginAsync(login.Email, login.Password))
                .ThrowsAsync(new ArgumentException("Missing credentials"))
                .Verifiable();

            var result = await _controller.Login(login);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing credentials", bad.Value);
            _mockUserService.Verify();
        }

        [Fact]
        public async Task Login_ReturnsOk_WithLoginResponseDto()
        {
            // Arrange
            var loginReq = new LoginRequestDTO
            {
                Email = "bob@x.com",
                Password = "valid123"
            };
            var expectedJwt = "JWT.TOKEN";
            var expectedRefresh = Guid.NewGuid().ToString();
            var expectedUserId = "bob-id";

            _mockUserService
                .Setup(s => s.LoginAsync(loginReq.Email, loginReq.Password))
                .ReturnsAsync((expectedJwt, expectedRefresh, expectedUserId))
                .Verifiable();

            // Act
            var action = await _controller.Login(loginReq);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action);
            var resp = Assert.IsType<LoginResponseDTO>(ok.Value);
            Assert.Equal(expectedJwt, resp.AuthToken);
            Assert.Equal(expectedRefresh, resp.RefreshToken);
            Assert.Equal(expectedUserId, resp.UserId);

            _mockUserService.Verify();
        }

        #endregion

        #region Logout
        [Fact]
        public async Task Logout_ReturnsNoContent_AndCallsService()
        {
            // Arrange
            _mockUserService
                .Setup(s => s.LogoutAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            // need an authenticated user to hit the [Authorize] path
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "u1") }, "MockAuth"))
                }
            };

            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockUserService.Verify();
        }
        #endregion
    }
}
