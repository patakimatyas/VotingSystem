using Azure.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VotingSystem.Shared.DTOs;
using VotingSystem.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace VotingSystem.DataAccess.Services
{
    public class PollService : IPollService
    {
        private readonly VotingSystemDbContext _context;

        public PollService(VotingSystemDbContext context)
        {
            _context = context;
        }

        public async Task CreatePollAsync(Poll poll)
        {
            if (string.IsNullOrWhiteSpace(poll.Question))
                throw new ArgumentException("Poll question cannot be empty.");

            if (poll.Options == null || poll.Options.Count < 2)
                throw new ArgumentException("A poll must have at least 2 options.");

            if (poll.Options.Any(o => string.IsNullOrWhiteSpace(o.Text)))
                throw new ArgumentException("Poll options must have text.");

            if (poll.StartDate > poll.EndDate)
                throw new ArgumentException("Start date cannot be after end date.");


            _context.Polls.Add(poll);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Poll>> GetAllPollsAsync()
        {
            return await _context.Polls
            .Include(p => p.Options)
        .   ToListAsync();
        }

        public async Task<List<Poll>> GetActivePollsAsync()
        {
            var now = DateTime.UtcNow;

            return await _context.Polls
                .Include(p => p.Options)
                .Where(p => p.StartDate <= now && p.EndDate >= now)
                .OrderBy(p => p.EndDate)
                .ToListAsync();
        }

        public async Task<Poll?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            return await _context.Polls
                .Include(p => p.Options)
                .Include(p => p.Votes)
                .Include(p => p.Voters)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Poll>> GetClosedPollsAsync(string? text, DateTime? from, DateTime? to)
        {
            var query = _context.Polls
                .Include(p => p.Options)
                .Where(p => p.EndDate < DateTime.Now)
                .AsQueryable();

            if (!string.IsNullOrEmpty(text))
                query = query.Where(p => p.Question.Contains(text));

            if (from.HasValue)
                query = query.Where(p => p.StartDate >= from.Value);

            if (to.HasValue)
                query = query.Where(p => p.EndDate < to.Value.AddDays(1));

            return await query.OrderByDescending(p => p.EndDate).ToListAsync();
        }

        public async Task<List<Poll>> GetPollsByUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId cannot be empty", nameof(userId));

            return await _context.Polls
                .Where(p => p.CreatedByUserId == userId)
                .Include(p => p.Options)
                .Include(p => p.Votes)
                .ToListAsync();
        }



    }
}
