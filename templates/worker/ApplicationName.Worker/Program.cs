using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using ApplicationName.Worker.Application;
using ApplicationName.Worker.Application.Services;
using ApplicationName.Worker.Consumers;
using ApplicationName.Worker.Contracts.Commands;
using ApplicationName.Worker.Infrastructure;
using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Events;
using Mapster;
using Conveyo;
using Conveyo.Diagnostics;
using Conveyo.RabbitMQ;
using Npgsql;
using Other.Worker.Contracts.Commands;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
#if DEBUG
using DotNetEnv;
using DotNetEnv.Configuration;
#endif
using StackExchange.Redis;

namespace ApplicationName.Worker;

[ExcludeFromCodeCoverage]
public static class Program
{
    private const string ServiceName = "ApplicationName.Worker";

    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);

#if DEBUG
        builder.ConfigureAppConfiguration(config =>
            config.AddDotNetEnv(".env", LoadOptions.TraversePath()));
#endif

        builder.ConfigureLogging(ConfigureLogging);

        builder.ConfigureServices(ConfigureServices);

        var app = builder.Build();

        await DatabaseInitializer.InitializeAsync(app.Services.GetRequiredService<NpgsqlDataSource>());

        await app.RunAsync();
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        var configuration = context.Configuration;

        // PostgreSQL
        services.AddSingleton(_ => new NpgsqlDataSourceBuilder(configuration["postgres:connection-string"]).Build());

        // Valkey
        var redisConfiguration = ConfigurationOptions.Parse(configuration["valkey:connection-string"]!, true);
        redisConfiguration.AbortOnConnectFail = false;
        redisConfiguration.ConnectRetry = 5;
        redisConfiguration.ConnectTimeout = 5000;
        redisConfiguration.SyncTimeout = 5000;
        redisConfiguration.KeepAlive = 30;
        redisConfiguration.Protocol = RedisProtocol.Resp3;

        var multiplexer = ConnectionMultiplexer.Connect(redisConfiguration);
        services.AddSingleton<IConnectionMultiplexer>(_ => multiplexer);

        // Mapster
        services.AddMapster();
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

        // Conveyo
        services.AddConveyo(bus =>
        {
            // Message type URNs. Commands/events shared with the API must use the same URN on both sides
            bus.Map<CreateExampleCommand>("applicationname:CreateExampleCommand.v1");
            bus.Map<UpdateExampleCommand>("applicationname:UpdateExampleCommand.v1");
            bus.Map<SetExampleRemoteCodeCommand>("applicationname:SetExampleRemoteCodeCommand.v1");
            bus.Map<ExampleCreatedEvent>("applicationname:ExampleCreatedEvent.v1");
            bus.Map<ExampleUpdatedEvent>("applicationname:ExampleUpdatedEvent.v1");
            bus.Map<ExampleRemoteCodeSetEvent>("applicationname:ExampleRemoteCodeSetEvent.v1");
            bus.Map<ExternalEvent>("other:ExternalEvent.v1");

            var uri = new Uri($"queue:{ServiceName}");
            bus.MapEndpointConvention<SetExampleRemoteCodeCommand>(uri);

            bus.AddConsumer<ExternalEventHandler>();
            bus.AddConsumer<ExampleCommandHandler>();

            bus.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["rabbitmq:host"]!, configuration["rabbitmq:vhost"]!, h =>
                {
                    h.Username(configuration["rabbitmq:username"]!);
                    h.Password(configuration["rabbitmq:password"]!);
                });

                cfg.ReceiveEndpoint($"{typeof(Program).Namespace}", e =>
                {
                    e.ConfigureConsumer<ExternalEventHandler>(ctx);
                    e.ConfigureConsumer<ExampleCommandHandler>(ctx);
                });
            });
        });

        // Application
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IProtoCacheRepository, ProtoCacheRepository>();
        services.AddScoped<IExampleService, ExampleService>();

        // Background Services
        services.AddHostedService<CacheInvalidationService>();

        // OpenTelemetry
        var otlpEndpoint = new Uri(configuration["opentelemetry:endpoint"]!);
        var appResourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(ServiceName, autoGenerateServiceInstanceId: false, serviceInstanceId: Dns.GetHostName());

        services.AddOpenTelemetry()
            .WithTracing(cfg => cfg
                .SetResourceBuilder(appResourceBuilder)
                .AddSource(DiagnosticHeaders.DefaultListenerName) // Conveyo
                .AddNpgsql()
                .AddOtlpExporter(configure => configure.Endpoint = otlpEndpoint))
            .WithMetrics(cfg => cfg
                .SetResourceBuilder(appResourceBuilder)
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(configure => configure.Endpoint = otlpEndpoint)
        );
    }

    private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
    {
        var configuration = context.Configuration;

        builder.AddOpenTelemetry(configure =>
        {
            configure.IncludeScopes = true;
            configure.ParseStateValues = true;
            configure.IncludeFormattedMessage = true;
            configure.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(ServiceName, autoGenerateServiceInstanceId: false, serviceInstanceId: Dns.GetHostName()))
                .AddOtlpExporter(opts => opts.Endpoint = new Uri(configuration["opentelemetry:endpoint"]!));
        });
    }
}
