using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VotingSystem.Blazor.Infrastructure;
using VotingSystem.Blazor.Services;

namespace VotingSystem.Blazor
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBlazorServices(this IServiceCollection services, IConfiguration configuration)
        {
            // HttpClient beállítása
            services.AddScoped<IHttpRequestUtility, HttpRequestUtility>();
            services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7189/") });

            // AuthService és társai

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IPollService, PollService>();

            // AutoMapper profil
            services.AddAutoMapper(typeof(BlazorMappingProfile));

            // localStorage használata
            services.AddBlazoredLocalStorage();

            // auth state kezeléshez
            services.AddAuthorizationCore();

            return services;
        }
    }
}
