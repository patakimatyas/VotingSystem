using VotingSystem.Shared.DTOs;
using VotingSystem.DataAccess.Models;

namespace VotingSystem.DataAccess.Services
{
    public interface IPollService
    {
        Task CreatePollAsync(Poll poll);
        Task<List<Poll>> GetAllPollsAsync();
        Task<List<Poll>> GetActivePollsAsync();
        Task<Poll?> GetByIdAsync(int id);
        Task<List<Poll>> GetClosedPollsAsync(string? text, DateTime? from, DateTime? to);
        Task<List<Poll>> GetPollsByUserAsync(string userId);
    }
}
