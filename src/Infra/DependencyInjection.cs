using Application.Interfaces;
using Infra.DataSources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infra.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using StatsdClient;

namespace Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        var mongoConnectionString = configuration["MongoDB:ConnectionString"]
            ?? throw new InvalidOperationException("MongoDB:ConnectionString não configurado.");
        var mongoDatabaseName = configuration["MongoDB:DatabaseName"] ?? "os";

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
        services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabaseName));

        services.AddScoped<IOrdemServicoDataSource, OrdemServicoDataSource>();

        services.AddSingleton<IEventPublisher>(_ =>
            RabbitMQEventPublisher.CreateAsync(configuration).GetAwaiter().GetResult());

        var dogStatsdConfig = new StatsdConfig
        {
            StatsdServerName = configuration["Datadog:StatsdHost"] ?? "localhost",
            StatsdPort = int.Parse(configuration["Datadog:StatsdPort"] ?? "8125")
        };
        var statsd = new DogStatsdService();
        statsd.Configure(dogStatsdConfig);
        services.AddSingleton<IDogStatsd>(statsd);
        services.AddSingleton<IMetricsService, DatadogMetricsService>();

        services.AddHostedService<RabbitMQConsumerService>();

        return services;
    }
}
