using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VotingSystem.Blazor.Components;
using VotingSystem.Blazor.Pages;
using VotingSystem.Blazor.Services;
using VotingSystem.Blazor.ViewModels;
using Xunit;

namespace VotingSystem.Blazor.Tests.Components
{
    public class PollDetailsComponentTests : IDisposable
    {
        private readonly TestContext _ctx = new();
        private readonly Mock<IPollService> _pollServiceMock = new();

        public PollDetailsComponentTests()
        {
            // register the PollService mock
            _ctx.Services.AddSingleton<IPollService>(_pollServiceMock.Object);
            // grab the fake nav manager so we can inspect navigation if needed
        }

        public void Dispose() => _ctx.Dispose();

        [Fact]
        public void PollDetails_ShowsLoading_WhenNoPollId()
        {
            // Arrange & Act: render with no parameters
            var cut = _ctx.RenderComponent<PollDetails>();

            // Assert: we see the "Loading poll..." placeholder
            var p = cut.Find("p");
            Assert.Equal("Loading poll...", p.TextContent.Trim());
        }

        [Fact]
        public void PollDetails_ShowsData_WhenServiceReturnsPoll()
        {
            // Arrange
            var vm = new PollDetailsViewModel
            {
                Question = "Pick one?",
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2025, 1, 2),
                Options = new List<OptionViewModel>
                {
                    new() { Id = 1, Text = "Option A", VoteCount = 3 },
                    new() { Id = 2, Text = "Option B", VoteCount = 5 },
                }
            };
            // the component parameter PollId is a string
            _pollServiceMock
                .Setup(s => s.GetPollAsync("42"))
                .ReturnsAsync(vm);

            // Act: render with PollId = "42"
            var cut = _ctx.RenderComponent<PollDetails>(parameters => parameters
                .Add(p => p.PollId, "42"));

            // Assert: question
            var h2 = cut.Find("h2");
            Assert.Equal("Pick one?", h2.TextContent.Trim());

            // Assert: dates
            var paras = cut.FindAll("p");
               // The component renders: "Start: 2025. 01. 01."
            var startFmt = vm.StartDate.ToString("yyyy.MM.dd.");
            var endFmt = vm.EndDate.ToString("yyyy.MM.dd.");
            Assert.Contains(startFmt, paras[0].TextContent);
            Assert.Contains(endFmt, paras[1].TextContent);

            // Assert: options rendered as buttons
            var optionItems = cut.FindAll("li.option-item");
            Assert.Equal(2, optionItems.Count);
            Assert.Contains("Option A", optionItems[0].TextContent);
            Assert.Contains("3 vote(s)", optionItems[0].TextContent);
            Assert.Contains("Option B", optionItems[1].TextContent);
            Assert.Contains("5 vote(s)", optionItems[1].TextContent);
        }
    }
}
