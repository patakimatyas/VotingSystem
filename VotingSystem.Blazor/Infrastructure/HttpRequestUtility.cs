using Blazored.LocalStorage;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace VotingSystem.Blazor.Infrastructure
{
    public class HttpRequestUtility : IHttpRequestUtility
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorageService;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public HttpRequestUtility(HttpClient httpClient, ILocalStorageService localStorageService)
        {
            _httpClient = httpClient;
            _localStorageService = localStorageService;
        }

        private async Task AddAuthorizationHeaderAsync()
        {
            var token = await _localStorageService.GetItemAsync<string>("AuthToken");
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<T?> ExecuteGetHttpRequestAsync<T>(string url)
        {
            await AddAuthorizationHeaderAsync();

            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedAsync();
                return default;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<T?> ExecutePostHttpRequestAsync<TIn, T>(string url, TIn data)
        {
            await AddAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync(url, data);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedAsync();
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task ExecutePostHttpRequestAsync(string url)
        {
            await AddAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsync(url, null);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedAsync();
            }
            response.EnsureSuccessStatusCode();
        }

        public async Task ExecutePostHttpRequestAsync<TIn>(string url, TIn content)
        {
            await AddAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync(url, content, _jsonOptions);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedAsync();
            }
            response.EnsureSuccessStatusCode();
        }

        private async Task HandleUnauthorizedAsync()
        {
            await _localStorageService.RemoveItemAsync("AuthToken");
            await _localStorageService.RemoveItemAsync("UserId");
            _httpClient.DefaultRequestHeaders.Authorization = null;

            throw new UnauthorizedAccessException("Session expired. Please login again.");
        }
    }
}