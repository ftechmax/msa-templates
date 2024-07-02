using System.Diagnostics.CodeAnalysis;
using System.Net;
using ApplicationName.Worker.Application;
using ApplicationName.Worker.Application.Services;
using ApplicationName.Worker.Consumers;
using ApplicationName.Worker.Contracts.Commands;
using ApplicationName.Worker.Infrastructure;
using MassTransit;
using MassTransit.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ApplicationName.Worker;

[ExcludeFromCodeCoverage]
public static class Program
{
    private const string ServiceName = "ApplicationName.Worker";

    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);

        builder.ConfigureLogging(ConfigureLogging);

        builder.ConfigureServices(ConfigureServices);

        var app = builder.Build();

        await app.RunAsync();
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        var configuration = context.Configuration;

        // MongoDB
        var clientSettings = MongoClientSettings.FromUrl(new MongoUrl(configuration["mongodb:connection-string"]));
        clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
        services.AddSingleton<IMongoClient>(_ => new MongoClient(clientSettings));

        // AutoMapper
        services.AddAutoMapper(i => i.AddProfile<MappingProfile>());

        // MassTransit
        services.AddMassTransit(i =>
        {
            var uri = new Uri($"queue:{ServiceName}");
            EndpointConvention.Map<SetExampleRemoteCodeCommand>(uri);

            i.AddConsumer<ExternalEventHandler>();
            i.AddConsumer<CommandHandler>();

            i.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["rabbitmq:host"], configuration["rabbitmq:vhost"], h =>
                {
                    h.Username(configuration["rabbitmq:username"]!);
                    h.Password(configuration["rabbitmq:password"]!);
                });

                cfg.ReceiveEndpoint($"{typeof(Program).Namespace}", e =>
                {
                    e.ConfigureConsumer<ExternalEventHandler>(ctx);
                    e.ConfigureConsumer<CommandHandler>(ctx);
                });
            });
        });

        // Application
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IApplicationService, ApplicationService>();

        // OpenTelemetry
        var otlpEndpoint = new Uri(configuration["OpenTelemetry:Endpoint"]!);
        var appResourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(ServiceName, autoGenerateServiceInstanceId: false, serviceInstanceId: Dns.GetHostName());

        services.AddOpenTelemetry()
            .WithTracing(cfg => cfg
                .SetResourceBuilder(appResourceBuilder)
                .AddSource(DiagnosticHeaders.DefaultListenerName) // MassTransit
                .AddOtlpExporter(configure =>
                {
                    configure.Endpoint = otlpEndpoint;
                }))
            .WithMetrics(cfg => cfg
                .SetResourceBuilder(appResourceBuilder)
                .AddProcessInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(configure =>
                {
                    configure.Endpoint = otlpEndpoint;
                })
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
                .AddOtlpExporter(opts =>
                {
                    opts.Endpoint = new Uri(configuration["OpenTelemetry:Endpoint"]!);
                });
        });
    }
}