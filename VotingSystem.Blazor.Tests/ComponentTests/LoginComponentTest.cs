using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bunit;
using Bunit.TestDoubles;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VotingSystem.Blazor.Pages;
using VotingSystem.Blazor.Services;
using VotingSystem.Blazor.ViewModels;
using Xunit;
using Microsoft.AspNetCore.Components;

namespace VotingSystem.Blazor.Tests.Components
{
    public class LoginComponentTest : IDisposable
    {
        private readonly TestContext _ctx = new();
        private readonly Mock<IAuthService> _authService = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<ILocalStorageService> _localStorage = new();
        private readonly FakeNavigationManager _navMan;

        public LoginComponentTest()
        {
            _ctx.Services.AddSingleton<IAuthService>(_authService.Object);
            _ctx.Services.AddSingleton<IMapper>(_mapper.Object);
            _ctx.Services.AddSingleton<ILocalStorageService>(_localStorage.Object);
            _navMan = _ctx.Services.GetRequiredService<FakeNavigationManager>();
        }

        public void Dispose() => _ctx.Dispose();

        [Fact]
        public async Task Login_Successful_NavigatesToPolls()
        {
            _authService.Setup(x => x.LoginAsync(It.IsAny<LoginViewModel>()))
                        .ReturnsAsync(true);

            var cut = _ctx.RenderComponent<Login>();

            // type email
            var inputs = cut.FindAll("input");
            await inputs[0].ChangeAsync(new ChangeEventArgs { Value = "user@example.com" });
            inputs = cut.FindAll("input");

            // type password
            await inputs[1].ChangeAsync(new ChangeEventArgs { Value = "Secret123!" });
            var btn = cut.Find("button[type=submit]");

            // click
            await btn.ClickAsync(new MouseEventArgs());

            Assert.EndsWith("/polls", _navMan.Uri);
            _authService.Verify(x => x.LoginAsync(It.Is<LoginViewModel>(m =>
                m.Email == "user@example.com" && m.Password == "Secret123!")), Times.Once);
        }

        [Fact]
        public async Task Login_Fails_ShowsErrorMessage()
        {
            // Arrange
            _authService
              .Setup(x => x.LoginAsync(It.IsAny<LoginViewModel>()))
              .ReturnsAsync(false);

            var cut = _ctx.RenderComponent<Login>();

            // Act
            // 1) fill email, then re-find inputs
            var inputs = cut.FindAll("input");
            await inputs[0].ChangeAsync(new ChangeEventArgs { Value = "bad@user.com" });
            inputs = cut.FindAll("input");

            // 2) fill password, then re-find button
            await inputs[1].ChangeAsync(new ChangeEventArgs { Value = "wrongpass" });
            var btn = cut.Find("button[type=submit]");

            // 3) click
            await btn.ClickAsync(new MouseEventArgs());

            // Assert
            var errors = cut
              .FindAll("ul.custom-validation li")
              .Select(li => li.TextContent.Trim());
            Assert.Contains("Incorrect email or password!", errors);

            _authService.Verify(x => x.LoginAsync(It.IsAny<LoginViewModel>()), Times.Once);
        }
    }
}
