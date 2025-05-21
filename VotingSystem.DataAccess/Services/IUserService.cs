using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VotingSystem.DataAccess.Models;

namespace VotingSystem.DataAccess.Services
{
    public interface IUserService
    {
        Task RegisterUserAsync(ApplicationUser user, string password);
        Task<(string authToken, string refreshToken, string userId)> LoginAsync(string email, string password);
        Task LogoutAsync();
        string? GetCurrentUserId();
        Task<ApplicationUser?> GetCurrentUserAsync();
        Task<List<ApplicationUser>> GetAllAsync();

    }
}
