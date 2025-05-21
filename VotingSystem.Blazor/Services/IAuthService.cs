using VotingSystem.Blazor.ViewModels;

namespace VotingSystem.Blazor.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(LoginViewModel viewModel);
        Task LogoutAsync();
        Task<string?> GetCurrentUserAsync();
    }
}
