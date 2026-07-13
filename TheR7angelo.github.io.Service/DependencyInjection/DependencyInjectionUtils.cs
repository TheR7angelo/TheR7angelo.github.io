using Microsoft.Extensions.DependencyInjection;
using TheR7angelo.github.io.Service.Github;
using TheR7angelo.github.io.Service.Interface.Services;
using TheR7angelo.github.io.Service.Mapper;
using TheR7angelo.github.io.Service.Mapper.Interfaces;

namespace TheR7angelo.github.io.Service.DependencyInjection;

public static class DependencyInjectionUtils
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAllDependency()
        {
            services.AddMapper()
                .AddService();

            return services;
        }

        private IServiceCollection AddMapper()
        {
            services.AddScoped<IGithubDomainToGithubDto, GithubDomainToGithubDto>();

            return services;
        }

        private IServiceCollection AddService()
        {
            services.AddScoped<IGithubService, GithubService>();

            return services;
        }
    }
}