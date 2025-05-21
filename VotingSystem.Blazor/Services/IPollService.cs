using VotingSystem.Blazor.ViewModels;
using VotingSystem.Shared.DTOs;

namespace VotingSystem.Blazor.Services
{
    public interface IPollService
    {
        Task<List<PollViewModel>> GetMyPollsAsync();
        Task<PollDetailsViewModel?> GetPollAsync(string pollId);
        Task<bool> CreatePollAsync(CreatePollViewModel viewModel);
    }
}
