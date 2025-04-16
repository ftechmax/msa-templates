using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Application.Services;
using ApplicationName.Api.Consumers;
using ApplicationName.Api.Contracts;
using ApplicationName.Api.Infrastructure;
using ApplicationName.Api.Validators;
using ApplicationName.Shared.Commands;
using FluentValidation;
using FluentValidation.AspNetCore;
using Mapster;
using MassTransit;
using MassTransit.Logging;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;

namespace ApplicationName.Api;

[ExcludeFromCodeCoverage]
public static class Program
{
    private const string ServiceName = "ApplicationName.Api";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureLogging(builder.Logging, builder.Configuration);

        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        Configure(app);

        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // MongoDB
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        var clientSettings = MongoClientSettings.FromUrl(new MongoUrl(configuration["mongodb:connection-string"]));
        clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
        services.AddSingleton<IMongoClient>(_ => new MongoClient(clientSettings));

        // SignalR
        services.AddSignalR();

        // Mapster
        services.AddMapster();
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

        // MassTransit + RabbitMQ
        services.AddMassTransit(i =>
        {
            var uri = new Uri("queue:ApplicationName.Worker");
            EndpointConvention.Map<CreateExampleCommand>(uri);
            EndpointConvention.Map<UpdateExampleCommand>(uri);

            i.AddConsumer<LocalEventHandler>();

            i.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["rabbitmq:host"], configuration["rabbitmq:vhost"], h =>
                {
                    h.Username(configuration["rabbitmq:username"]!);
                    h.Password(configuration["rabbitmq:password"]!);
                });

                cfg.ReceiveEndpoint(ServiceName, e =>
                {
                    e.ConfigureConsumer<LocalEventHandler>(ctx);
                });
            });
        });

        // Redis
        var redisConfiguration = ConfigurationOptions.Parse(configuration["redis:connection-string"]!, true);
        services.AddSingleton<IConnectionMultiplexer, ConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConfiguration));
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["redis:connection-string"];
            options.InstanceName = $"{ApplicationConstants.ApplicationKey}:";
        });

        // Api
        services.AddHealthChecks();
        services.AddControllers();

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateExampleDtoValidator>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddFluentValidationRulesToSwagger();
        services.AddResponseCompression();

        // Application
        services.AddScoped<IExampleService, ExampleService>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IProtoCacheRepository, ProtoCacheRepository>();

        // OpenTelemetry
        var openTelemetryEndpoint = new Uri(configuration["OpenTelemetry:Endpoint"]!);
        var appResourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(ServiceName, autoGenerateServiceInstanceId: false, serviceInstanceId: Dns.GetHostName());

        services.AddOpenTelemetry()
            .WithTracing(builder => builder
                .SetResourceBuilder(appResourceBuilder)
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = req => !(req.Request.Path.Equals("/healthz") || req.Request.Path.Equals("/metrics") || req.Request.Path.StartsWithSegments("/swagger"));
                    options.RecordException = true;
                })
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                })
                .AddSource(DiagnosticHeaders.DefaultListenerName) // MassTransit
                .AddRedisInstrumentation()
                .AddOtlpExporter(configure =>
                {
                    configure.Endpoint = openTelemetryEndpoint;
                }))
            .WithMetrics(builder => builder
                .SetResourceBuilder(appResourceBuilder)
                .AddAspNetCoreInstrumentation()
                .AddProcessInstrumentation()
                .AddRuntimeInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(configure =>
                {
                    configure.Endpoint = openTelemetryEndpoint;
                })
        );
    }

    private static void Configure(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();
        app.UseResponseCompression();

        app.MapControllers();
        app.MapHealthChecks("/healthz");
        app.MapHub<ApiHub>("/api-hub");
    }

    private static void ConfigureLogging(ILoggingBuilder builder, IConfiguration configuration)
    {
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