using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VotingSystem.DataAccess;
using VotingSystem.DataAccess.Services;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Identity;
using VotingSystem.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using VotingSystem.DataAccess.Config;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging.Abstractions;

namespace VotingSystem.Tests.UnitTests
{
    public class UserServiceTest
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;

        private readonly IUserService _userService;

        public UserServiceTest()
        {
            var store = new Mock<IUserStore<ApplicationUser>>().Object;

            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<ApplicationUser>>>().Object
            );


            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                _mockUserManager.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
                Options.Create(new IdentityOptions()),
                new Mock<ILogger<SignInManager<ApplicationUser>>>().Object,
                new Mock<IAuthenticationSchemeProvider>().Object,
                new Mock<IUserConfirmation<ApplicationUser>>().Object

            );

            _mockUserManager
            .Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>());

            var jwtSettings = Options.Create(new JwtSettings
            {
                SecretKey = "mysuper_secret_signing_key_123456", // 32+ chars
                Issuer = "https://localhost",
                Audience = "https://localhost",
                ExpiryMinutes = 30
            });
            _userService = new UserService(
                _mockUserManager.Object,
                _mockSignInManager.Object,
                jwtSettings,
                new Mock<IHttpContextAccessor>().Object);
        }

        #region RegisterUserAsync

        [Fact]
        public async Task RegisterUserAsync_NullUser_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _userService.RegisterUserAsync(null!, "whatever"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task RegisterUserAsync_InvalidPassword_ThrowsArgumentException(string? pwd)
        {
            var user = new ApplicationUser { Email = "x@x.com", UserName = "u" };
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _userService.RegisterUserAsync(user, pwd!));

            Assert.Equal("password", ex.ParamName);
            Assert.Contains("Password cannot be empty", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task RegisterUserAsync_InvalidEmail_ThrowsArgumentException(string? email)
        {
            var user = new ApplicationUser { Email = email, UserName = "u" };
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _userService.RegisterUserAsync(user, "GoodPass1!"));

            Assert.Equal("Email", ex.ParamName);
            Assert.Contains("User must have an email", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task RegisterUserAsync_InvalidUserName_ThrowsArgumentException(string? uname)
        {
            var user = new ApplicationUser { Email = "x@x.com", UserName = uname };
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _userService.RegisterUserAsync(user, "GoodPass1!"));

            Assert.Equal("UserName", ex.ParamName);
            Assert.Contains("User must have a user name", ex.Message);
        }

        [Fact]
        public async Task RegisterUserAsync_ValidInputs_CallsCreateAndSetsRefreshToken()
        {
            var user = new ApplicationUser { Email = "x@x.com", UserName = "u" };
            const string pwd = "GoodPass1!";

            _mockUserManager
                .Setup(x => x.CreateAsync(user, pwd))
                .ReturnsAsync(IdentityResult.Success)
                .Verifiable();

            Assert.Null(user.RefreshToken);   // before registration

            await _userService.RegisterUserAsync(user, pwd);

            _mockUserManager.Verify(x => x.CreateAsync(user, pwd), Times.Once);
            Assert.NotEqual(Guid.Empty, user.RefreshToken);
        }

        [Fact]
        public async Task RegisterUserAsync_CreateFails_DuplicateError_ThrowsInvalidOperationException()
        {
            var user = new ApplicationUser { Email = "x@x.com", UserName = "u" };
            const string pwd = "GoodPass1!";
            const string err = "Email already taken";

            // simulate a duplicate identity error code
            var duplicateError = new IdentityError { Code = "DuplicateEmail", Description = err };
            _mockUserManager
                .Setup(x => x.CreateAsync(user, pwd))
                .ReturnsAsync(IdentityResult.Failed(duplicateError));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.RegisterUserAsync(user, pwd));

            Assert.Contains(err, ex.Message);
        }

        [Fact]
        public async Task RegisterUserAsync_CreateFails_NonDuplicateError_ThrowsArgumentException()
        {
            var user = new ApplicationUser { Email = "x@x.com", UserName = "u" };
            const string pwd = "GoodPass1!";
            const string err = "Some other identity failure";

            var genericError = new IdentityError { Code = "BadSomething", Description = err };
            _mockUserManager
                .Setup(x => x.CreateAsync(user, pwd))
                .ReturnsAsync(IdentityResult.Failed(genericError));

            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _userService.RegisterUserAsync(user, pwd));

            // ParamName is null since we call ArgumentException(string)
            Assert.Null(ex.ParamName);
            Assert.Contains(err, ex.Message);
        }

        #endregion

        #region LoginAsync

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LoginAsync_InvalidEmail_ThrowsArgumentException(string? email)
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _userService.LoginAsync(email!, "irrelevant"));
            Assert.Equal("email", ex.ParamName);
            Assert.Contains("Email cannot be empty", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LoginAsync_InvalidPassword_ThrowsArgumentException(string? pwd)
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _userService.LoginAsync("x@x.com", pwd!));
            Assert.Equal("password", ex.ParamName);
            Assert.Contains("Password cannot be empty", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ThrowsAccessViolationException()
        {
            const string email = "noone@nowhere";
            _mockUserManager
                .Setup(m => m.FindByEmailAsync(email))
                .ReturnsAsync((ApplicationUser)null!);

            var ex = await Assert.ThrowsAsync<AccessViolationException>(
                () => _userService.LoginAsync(email, "pwd"));
            Assert.Contains("Email or password is invalid", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_UserLockedOut_ThrowsAccessViolationException()
        {
            var user = new ApplicationUser { UserName = "bob", RefreshToken = Guid.NewGuid(), Id = "bob-id" };
            const string email = "bob@x.com";
            const string pwd = "pwd";

            _mockUserManager
                .Setup(m => m.FindByEmailAsync(email))
                .ReturnsAsync(user);
            _mockSignInManager
                .Setup(s => s.PasswordSignInAsync(user.UserName!, pwd, false, true))
                .ReturnsAsync(SignInResult.LockedOut);

            var ex = await Assert.ThrowsAsync<AccessViolationException>(
                () => _userService.LoginAsync(email, pwd));
            Assert.Contains("Too many failed attempt", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_InvalidCredentials_ThrowsAccessViolationException()
        {
            var user = new ApplicationUser { UserName = "alice", RefreshToken = Guid.NewGuid(), Id = "alice-id" };
            const string email = "alice@x.com";
            const string pwd = "pwd";

            _mockUserManager
                .Setup(m => m.FindByEmailAsync(email))
                .ReturnsAsync(user);
            _mockSignInManager
                .Setup(s => s.PasswordSignInAsync(user.UserName!, pwd, false, true))
                .ReturnsAsync(SignInResult.Failed);

            var ex = await Assert.ThrowsAsync<AccessViolationException>(
                () => _userService.LoginAsync(email, pwd));
            Assert.Contains("Email or password is invalid", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_Success_ReturnsJwtAndRefreshAndUserId()
        {
            // arrange
            var guid = Guid.NewGuid();
            var user = new ApplicationUser
            {
                UserName = "charlie",
                Email = "charlie@x.com",
                RefreshToken = guid,
                Id = "charlie-id"
            };
            const string pwd = "ValidPwd1!";

            _mockUserManager
                .Setup(m => m.FindByEmailAsync(user.Email!))
                .ReturnsAsync(user);
            _mockSignInManager
                .Setup(s => s.PasswordSignInAsync(user.UserName!, pwd, false, true))
                .ReturnsAsync(SignInResult.Success);

            // act
            var (token, refresh, id) = await _userService.LoginAsync(user.Email!, pwd);

            // assert
            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.Equal(guid.ToString(), refresh);
            Assert.Equal(user.Id, id);
        }

        #endregion
    }
}
