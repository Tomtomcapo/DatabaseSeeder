using Microsoft.Extensions.DependencyInjection;
using DatabaseSeeder.Core.Configuration;

namespace DatabaseSeeder.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static SeederBuilder AddDatabaseSeeder(this IServiceCollection services)
    {
        return new SeederBuilder(services);
    }

    public static IServiceCollection AddDatabaseSeeder(
        this IServiceCollection services,
        Action<SeederOptions> configureOptions)
    {
        return new SeederBuilder(services)
            .Configure(configureOptions)
            .Build();
    }
}