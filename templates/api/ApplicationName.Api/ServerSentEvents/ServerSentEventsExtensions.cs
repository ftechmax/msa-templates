namespace ApplicationName.Api.ServerSentEvents;

public static class ServerSentEventsExtensions
{
    public static IServiceCollection AddServerSentEvents(this IServiceCollection services)
    {
        services.AddSingleton<ServerSentEventsService>();
        services.AddSingleton<IServerSentEventsService>(i => i.GetRequiredService<ServerSentEventsService>());
        services.AddHostedService(i => i.GetRequiredService<ServerSentEventsService>());

        return services;
    }

    public static void MapServerSentEvents(this WebApplication app)
    {
        app.MapGet("/events", (IServerSentEventsService events, CancellationToken cancellationToken) =>
            TypedResults.ServerSentEvents(events.SubscribeAsync(cancellationToken)));
    }
}
