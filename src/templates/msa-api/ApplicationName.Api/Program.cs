using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Application.Services;
using ApplicationName.Api.Consumers;
using ApplicationName.Api.Contracts.Commands;
using ApplicationName.Api.Infrastructure;
using ApplicationName.Api.Validators;
using FluentValidation;
using MassTransit;
using MassTransit.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // MongoDB
        var connectionString = new MongoUrl($"mongodb://{configuration["mongodb:username"]}:{configuration["mongodb:password"]}@{configuration["mongodb:host"]}:27017");
        var clientSettings = MongoClientSettings.FromUrl(connectionString);
        clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
        services.AddSingleton<IMongoClient>(_ => new MongoClient(clientSettings));

        // AutoMapper
        services.AddAutoMapper(i => i.AddProfile<MappingProfile>());

        // MassTransit + RabbitMQ
        services.AddMassTransit(i =>
        {
            var uri = new Uri("queue:ApplicationName.Worker");
            EndpointConvention.Map<IExampleCommand>(uri);

            i.AddConsumer<LocalEventHandler>();

            i.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["rabbitmq:host"], configuration["rabbitmq:vhost"], h =>
                {
                    h.Username(configuration["rabbitmq:username"]);
                    h.Password(configuration["rabbitmq:password"]);
                });

                cfg.ReceiveEndpoint(ServiceName, e =>
                {
                    e.ConfigureConsumer<LocalEventHandler>(context);
                });
            });
        });

        // Redis
        var redisConfiguration = ConfigurationOptions.Parse(configuration["redis:config"]!, true);
        services.AddSingleton<IConnectionMultiplexer, ConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConfiguration));
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["redis:config"];
            options.InstanceName = $"{configuration["redis:instance"]}_";
        });

        // OpenTelemetry
        var appResourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(ServiceName, autoGenerateServiceInstanceId: false, serviceInstanceId: Dns.GetHostName());

        services.AddOpenTelemetry().WithTracing(builder => builder
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
                configure.Endpoint = new Uri(configuration["OpenTelemetry:Endpoint"]!);
            })
        );

        services.AddOpenTelemetry().WithMetrics(builder => builder
            .SetResourceBuilder(appResourceBuilder)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(configure =>
            {
                configure.Endpoint = new Uri(configuration["OpenTelemetry:Endpoint"]!);
            })
        );

        // Api
        services.AddHealthChecks();
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddResponseCompression();
        services.AddValidatorsFromAssemblyContaining<CreateExampleDtoValidator>();

        // Application
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IProtoCacheRepository, ProtoCacheRepository>();
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
    }
}