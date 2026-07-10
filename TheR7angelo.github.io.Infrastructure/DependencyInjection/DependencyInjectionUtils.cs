using Microsoft.Extensions.DependencyInjection;
using TheR7angelo.github.io.Infrastructure.Github;
using TheR7angelo.github.io.Infrastructure.Interface.Repositories;
using TheR7angelo.github.io.Infrastructure.Mapper;
using TheR7angelo.github.io.Infrastructure.Mapper.Interfaces;

namespace TheR7angelo.github.io.Infrastructure.DependencyInjection;

public static class DependencyInjectionUtils
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAllDependency()
        {
            services.AddGitHubHttpClient()
                .AddMapper()
                .AddRepository();

            return services;
        }

        private IServiceCollection AddGitHubHttpClient()
        {
            services.AddHttpClient(IGithubRepository.HttpGithubClientName, client =>
            {
                client.BaseAddress = new Uri("https://api.github.com/");
                client.DefaultRequestHeaders.Add("User-Agent", "TheR7angelo.github.io");
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            });

            return services;
        }

        private IServiceCollection AddMapper()
        {
            services.AddScoped<IGithubEntitiesToGithubDomain, GithubEntitiesToGithubDomain>();

            return services;
        }

        private IServiceCollection AddRepository()
        {
            services.AddScoped<IGithubRepository, GithubRepository>();

            return services;
        }
    }
}