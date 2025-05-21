using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bunit;
using Bunit.TestDoubles;
using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VotingSystem.Blazor.Components;
using VotingSystem.Blazor.Services;
using VotingSystem.Blazor.ViewModels;
using Xunit;
using VotingSystem.Blazor.Pages;

namespace VotingSystem.Blazor.Tests.Components
{
    public class PollsComponentTests : IDisposable
    {
        private readonly TestContext _ctx = new();
        private readonly Mock<IPollService> _pollService = new();
        private readonly FakeNavigationManager _navMan;
        private readonly BunitJSInterop _js;

        public PollsComponentTests()
        {
            _ctx.Services.AddSingleton<IPollService>(_pollService.Object);

            _ctx.Services.AddSingleton<ILocalStorageService>(Mock.Of<ILocalStorageService>());

            _js = _ctx.JSInterop;

            _navMan = _ctx.Services.GetRequiredService<FakeNavigationManager>();
        }

        public void Dispose() => _ctx.Dispose();

        [Fact]
        public void Polls_ShowsNoPollsMessage_WhenServiceReturnsEmpty()
        {
            _pollService
                .Setup(s => s.GetMyPollsAsync())
                .ReturnsAsync(new List<PollViewModel>());

            var cut = _ctx.RenderComponent<Polls>();

            var msg = cut.Find("div.mypolls-page p");
            Assert.Equal("You have not created any polls yet.", msg.TextContent.Trim());
        }

        [Fact]
        public void Polls_ShowsListOfPolls_WhenServiceReturnsData()
        {
            var polls = new List<PollViewModel>
            {
                new() { Id = 1, Question = "First?",  StartDate = DateTime.Parse("2025-01-01"), EndDate = DateTime.Parse("2025-01-02") },
                new() { Id = 2, Question = "Second?", StartDate = DateTime.Parse("2025-02-01"), EndDate = DateTime.Parse("2025-02-02") }
            };
            _pollService
                .Setup(s => s.GetMyPollsAsync())
                .ReturnsAsync(polls);

            var cut = _ctx.RenderComponent<Polls>();

            var cards = cut.FindAll("li.poll-card");
            Assert.Equal(2, cards.Count);

            var first = cards[0];
            var header = first.QuerySelector("h3");
            Assert.Equal("First?", header!.TextContent.Trim());

            var paras = first
                          .QuerySelectorAll("p")
                          .Select(p => p.TextContent)
                          .ToArray();
            Assert.Contains("2025. 01. 01.", paras[0]);
            Assert.Contains("2025. 01. 02.", paras[1]);

            first.Click();
            Assert.EndsWith("/polls/1", _navMan.Uri);
        }

        [Fact]
        public void Polls_RedirectsToLoginAndAlerts_WhenUnauthorized()
        {
            // tell bUnit to handle the alert call
            _js.SetupVoid("alert", _ => true);

            _pollService
                .Setup(s => s.GetMyPollsAsync())
                .ThrowsAsync(new UnauthorizedAccessException());

            var cut = _ctx.RenderComponent<Polls>();

            // verify the alert was invoked
            var call = _js.VerifyInvoke("alert");
            Assert.Equal("Session expired. Please login again.", call.Arguments[0]?.ToString());

            Assert.EndsWith("/login", _navMan.Uri);
        }

        [Fact]
        public void Polls_AlertsUnexpectedError_WhenHttpRequestException()
        {
            // tell bUnit to handle the alert call
            _js.SetupVoid("alert", _ => true);

            _pollService
                .Setup(s => s.GetMyPollsAsync())
                .ThrowsAsync(new HttpRequestException());

            var cut = _ctx.RenderComponent<Polls>();

            var call = _js.VerifyInvoke("alert");
            Assert.Equal("Unexpected error.", call.Arguments[0]?.ToString());

            // because _polls remains null, the loading message still shows
            var loading = cut.Find("div.mypolls-page p");
            Assert.Equal("Loading...", loading.TextContent.Trim());
        }
    }
}
