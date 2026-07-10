namespace TheR7angelo.github.io.DependencyInjection;

public static class DependencyInjectionUtils
{
    public static IServiceCollection AddAllDependency(this IServiceCollection services)
    {
        Infrastructure.DependencyInjection.DependencyInjectionUtils.AddAllDependency(services);
        Service.DependencyInjection.DependencyInjectionUtils.AddAllDependency(services);

        return services;
    }
}