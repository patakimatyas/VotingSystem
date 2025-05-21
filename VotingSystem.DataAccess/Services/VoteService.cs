using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VotingSystem.DataAccess.Models;

namespace VotingSystem.DataAccess.Services
{
    public class VoteService : IVoteService
    {

        private readonly VotingSystemDbContext _context;
        public VoteService(VotingSystemDbContext context)
        {
            _context = context;
        }

        public async Task VoteAsync(int pollId, int optionId, string userId)
        {

            var poll = await _context.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == pollId);

            if (poll == null || poll.IsClosed)
            {
                throw new ArgumentException("Poll not found or is closed");
            }

            if (!poll.Options.Any(o => o.Id == optionId))
            {
                throw new ArgumentException("Invalid option for this poll");
            }

            // Check if user has already voted
            if (await _context.Voters.AnyAsync(v => v.UserId == userId && v.PollId == pollId))
                throw new InvalidOperationException("You have already voted in this poll.");

            // Record the user's participation
            _context.Voters.Add(new Voter { UserId = userId, PollId = pollId });

            // Record the anonymous vote
            _context.Votes.Add(new Vote { PollId = pollId, OptionId = optionId });

            await _context.SaveChangesAsync();
        }


        public async Task<bool> HasVotedAsync(string userId, int pollId)
        {
            return await _context.Voters
                .AnyAsync(v => v.UserId == userId && v.PollId == pollId);
        }

        public async Task<List<int>> GetVotedPollIdsAsync(string userId)
        {
            return await _context.Voters
                .Where(v => v.UserId == userId)
                .Select(v => v.PollId)
                .ToListAsync();
        }

    }
}
