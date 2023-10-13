using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using ApplicationName.Worker.Application;
using ApplicationName.Worker.Application.Services;
using ApplicationName.Worker.Consumers;
using ApplicationName.Worker.Contracts.Commands;
using ApplicationName.Worker.Infrastructure;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
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

        var connectionString = $"mongodb://{configuration["mongodb:username"]}:{configuration["mongodb:password"]}@{configuration["mongodb:host"]}:27017";
        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));

        services.AddAutoMapper(i => i.AddProfile<MappingProfile>());

        // MassTransit
        services.AddMassTransit(i =>
        {
            EndpointConvention.Map<IExampleCommand>(new Uri($"queue:{typeof(Program).Namespace}"));

            i.AddConsumer<ExternalEventHandler>();
            i.AddConsumer<CommandHandler>();

            i.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["rabbitmq:host"], configuration["rabbitmq:vhost"], h =>
                {
                    h.Username(configuration["rabbitmq:username"]);
                    h.Password(configuration["rabbitmq:password"]);
                });

                cfg.ReceiveEndpoint($"{typeof(Program).Namespace}", e =>
                {
                    e.ConfigureConsumer<ExternalEventHandler>(context);
                    e.ConfigureConsumer<CommandHandler>(context);
                });
            });
        });

        // OpenTelemetry
        var appResourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(ServiceName, autoGenerateServiceInstanceId: false, serviceInstanceId: Dns.GetHostName());

        services.AddOpenTelemetry().WithTracing(cfg => cfg
            .SetResourceBuilder(appResourceBuilder)
            .AddSource(DiagnosticHeaders.DefaultListenerName) // MassTransit
            .AddMongoDBInstrumentation()
            .AddOtlpExporter(configure =>
            {
                configure.Endpoint = new Uri(configuration["OpenTelemetry:Endpoint"]);
            })
        );

        services.AddOpenTelemetry().WithMetrics(cfg => cfg
            .SetResourceBuilder(appResourceBuilder)
            .AddRuntimeInstrumentation()
            .AddOtlpExporter(configure =>
            {
                configure.Endpoint = new Uri(configuration["OpenTelemetry:Endpoint"]);
            })
        );

        // Application
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IApplicationService, ApplicationService>();
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
                    opts.Endpoint = new Uri(configuration.GetValue<string>("OpenTelemetry:Endpoint"));
                });
        });
    }
}