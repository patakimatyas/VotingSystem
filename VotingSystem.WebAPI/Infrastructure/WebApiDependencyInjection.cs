using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace VotingSystem.WebAPI.Infrastructure
{
    public static class WebApiDependencyInjection
    {
        public static IServiceCollection AddWebApiAutoMapper(this IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new WebApiMappingProfile());
            });

            mapperConfig.AssertConfigurationIsValid();

            services.AddAutoMapper(typeof(WebApiMappingProfile).Assembly);
            return services;
        }
    }
}
