using Microsoft.Extensions.DependencyInjection;
using DatabaseSeeder.Core.Abstractions;
using DatabaseSeeder.Core.Implementation;

namespace DatabaseSeeder.Core.Configuration;

public class SeederBuilder
{
    private readonly IServiceCollection _services;
    private readonly SeederOptions _options;

    public IServiceCollection Services => _services;

    public SeederBuilder(IServiceCollection services)
    {
        _services = services;
        _options = new SeederOptions();
    }

    public SeederBuilder Configure(Action<SeederOptions> configureOptions)
    {
        configureOptions(_options);
        return this;
    }

    public SeederBuilder AddSeeder<TSeeder>() where TSeeder : class, ISeeder
    {
        _services.AddScoped<ISeeder, TSeeder>();
        return this;
    }

    public SeederBuilder AddDataSeeder<TEntity, TDataSeeder>()
        where TEntity : class
        where TDataSeeder : class, IDataSeeder<TEntity>
    {
        _services.AddScoped<IDataSeeder<TEntity>, TDataSeeder>();
        return this;
    }

    public IServiceCollection Build()
    {
        _services.AddSingleton(_options);
        _services.AddScoped<ISeederRegistry, SeederRegistry>();
        _services.AddScoped<ISeederOrchestrator, SeederOrchestrator>();
        return _services;
    }
}