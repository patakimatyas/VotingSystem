using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingSystem.DataAccess.Services
{
    public interface IVoteService
    {
        Task VoteAsync(int pollId, int optionId, string userId);
        Task<bool> HasVotedAsync(string userId, int pollId);
        Task<List<int>> GetVotedPollIdsAsync(string userId);
        
    }
}
