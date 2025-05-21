using AutoMapper;
using Blazored.LocalStorage;
using VotingSystem.Blazor.Infrastructure;
using VotingSystem.Blazor.ViewModels;
using VotingSystem.Shared.DTOs;

namespace VotingSystem.Blazor.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpRequestUtility _http;
        private readonly ILocalStorageService _localStorage;
        private readonly IMapper _mapper;

        public AuthService(
            IHttpRequestUtility http,
            ILocalStorageService localStorage,
            IMapper mapper)
        {
            _http = http;
            _localStorage = localStorage;
            _mapper = mapper;
        }

        public async Task<bool> LoginAsync(LoginViewModel viewModel)
        {
            var dto = _mapper.Map<LoginRequestDTO>(viewModel);

            try
            {
                var result = await _http.ExecutePostHttpRequestAsync<LoginRequestDTO, LoginResponseDTO>(
                    "api/users/login", dto);

                if(result is not null)
                {
                    await _localStorage.SetItemAsync("AuthToken", result.AuthToken);
                    await _localStorage.SetItemAsync("UserId", result.UserId);
                }
                return true;

            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _http.ExecutePostHttpRequestAsync("api/users/logout");
            }
            catch (HttpRequestException) { }

            await _localStorage.RemoveItemAsync("AuthToken");
            await _localStorage.RemoveItemAsync("UserId");
            await _localStorage.RemoveItemAsync("UserName");

        }

        public async Task<string?> GetCurrentUserAsync()
        {
            return await _localStorage.GetItemAsync<string>("UserName");
        }

    }
}
