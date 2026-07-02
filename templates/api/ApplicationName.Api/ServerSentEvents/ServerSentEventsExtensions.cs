namespace ApplicationName.Api.ServerSentEvents;

public static class ServerSentEventsExtensions
{
    public static IServiceCollection AddServerSentEvents(this IServiceCollection services)
    {
        return services.AddSingleton<IServerSentEventsService, ServerSentEventsService>();
    }

    public static void MapServerSentEvents(this WebApplication app)
    {
        app.MapGet("/events", (IServerSentEventsService events, CancellationToken cancellationToken) =>
            TypedResults.ServerSentEvents(events.SubscribeAsync(cancellationToken)));
    }
}
