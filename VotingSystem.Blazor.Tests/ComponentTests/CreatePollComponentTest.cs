using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VotingSystem.Blazor.Components;
using VotingSystem.Blazor.Pages;
using VotingSystem.Blazor.Services;
using VotingSystem.Blazor.ViewModels;
using Xunit;

namespace VotingSystem.Blazor.Tests.Components
{
    public class CreatePollComponentTests : IDisposable
    {
        private readonly TestContext _ctx = new();
        private readonly Mock<IPollService> _pollService = new();
        private readonly Mock<IJSRuntime> _jsRuntime = new();
        private readonly Mock<ILocalStorageService> _localStorage = new();
        private readonly FakeNavigationManager _navMan;

        public CreatePollComponentTests()
        {
            // register fakes & mocks
            _ctx.Services.AddSingleton<ILocalStorageService>(_localStorage.Object);
            _ctx.Services.AddSingleton(_pollService.Object);
            _ctx.Services.AddSingleton(_jsRuntime.Object);
            _localStorage
     .          Setup(ls => ls.GetItemAsStringAsync(
                   It.Is<string>(key => key == "AuthToken"),
                   It.IsAny<CancellationToken>()))
     .          ReturnsAsync("fake-jwt-token");
            // resolve the built-in FakeNavigationManager
            _navMan = _ctx.Services.GetRequiredService<FakeNavigationManager>();


        }

        public void Dispose() => _ctx.Dispose();

        [Fact]
        public void AddOption_Click_ShouldAddNewOptionInput()
        {
            // Arrange: render and grab initial count (should be 2 by default)
            var cut = _ctx.RenderComponent<CreatePoll>();
            var before = cut.FindAll("[data-testid=option-input]").Count;
            Assert.Equal(2, before);

            // Act (inside InvokeAsync so bUnit processes the re-render)
            cut.InvokeAsync(() =>
            {
                cut.Find("button.option-button").Click();
            });

            // Assert: now there’s exactly one more
            var after = cut.FindAll("[data-testid=option-input]").Count;
            Assert.Equal(before + 1, after);
        }

        [Fact]
        public async Task HandleCreate_InvalidDates_ShowsStartBeforeEndError()
        {
            var cut = _ctx.RenderComponent<CreatePoll>();

            await cut.InvokeAsync(() =>
            {
                // swap dates
                cut.Find("[data-testid=start-date]").Change("2025-12-31");
                cut.Find("[data-testid=end-date]").Change("2025-01-01");
                // submit
                cut.Find("[data-testid=submit]").Click();
            });

            var summary = cut.Find("[data-testid=validation-summary]");
            Assert.Contains("Start date must be before end date.", summary.TextContent);
        }

        [Fact]
        public async Task HandleCreate_TooFewOptions_ShowsOptionsError()
        {
            var cut = _ctx.RenderComponent<CreatePoll>();

            await cut.InvokeAsync(() =>
            {
                // clear all option texts
                foreach (var inp in cut.FindAll("[data-testid=option-input]"))
                    inp.Change(string.Empty);

                // valid dates
                cut.Find("[data-testid=start-date]").Change("2025-01-01");
                cut.Find("[data-testid=end-date]").Change("2025-12-31");

                // submit
                cut.Find("[data-testid=submit]").Click();
            });

            var summary = cut.Find("[data-testid=validation-summary]");
            Assert.Contains("At least two non-empty options are required.", summary.TextContent);
        }

        [Fact]
        public async Task HandleCreate_ValidModel_CallsServiceAndNavigatesToPolls()
        {
            _pollService
                .Setup(s => s.CreatePollAsync(It.IsAny<CreatePollViewModel>()))
                .ReturnsAsync(true);

            var cut = _ctx.RenderComponent<CreatePoll>();

            await cut.InvokeAsync(() => cut.Find("[data-testid=question-input]").Change("A valid question?"));
            // Set option texts
            await cut.InvokeAsync(() => cut.FindAll("[data-testid=option-input]")[0].Change("Q1"));
            await cut.InvokeAsync(() => cut.FindAll("[data-testid=option-input]")[1].Change("Q2"));

            // Set valid dates
            await cut.InvokeAsync(() => cut.Find("[data-testid=start-date]").Change("2025-01-01"));
            await cut.InvokeAsync(() => cut.Find("[data-testid=end-date]").Change("2025-12-31"));

            // Submit
            await cut.InvokeAsync(() => cut.Find("[data-testid=submit]").Click());

            _pollService.Verify(s => s.CreatePollAsync(It.IsAny<CreatePollViewModel>()), Times.Once);
            Assert.Equal("http://localhost/polls", _navMan.Uri);
        }


        [Fact]
        public async Task HandleCreate_Unauthorized_NavigatesToLogin()
        {
            _pollService
                .Setup(s => s.CreatePollAsync(It.IsAny<CreatePollViewModel>()))
                .Throws<UnauthorizedAccessException>();

            var cut = _ctx.RenderComponent<CreatePoll>();

            await cut.InvokeAsync(() => cut.Find("[data-testid=question-input]").Change("A valid question?"));

            // Set option texts
            await cut.InvokeAsync(() => cut.FindAll("[data-testid=option-input]")[0].Change("A"));
            await cut.InvokeAsync(() => cut.FindAll("[data-testid=option-input]")[1].Change("B"));

            // Set valid dates
            await cut.InvokeAsync(() => cut.Find("[data-testid=start-date]").Change("2025-01-01"));
            await cut.InvokeAsync(() => cut.Find("[data-testid=end-date]").Change("2025-12-31"));

            // Submit
            await cut.InvokeAsync(() => cut.Find("[data-testid=submit]").Click());

            Assert.Equal("http://localhost/login", _navMan.Uri);
        }





    }
}
