using System.Threading.Tasks;
using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Contracts;
using ApplicationName.Api.Contracts.Events;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace ApplicationName.Api.Consumers;

public class LocalEventHandler : IConsumer<IExampleEvent>
{
    private readonly IHubContext<ApiHub> _hub;

    private readonly IProtoCacheRepository _protoCacheRepository;

    public LocalEventHandler(IHubContext<ApiHub> hub, IProtoCacheRepository protoCacheRepository)
    {
        _hub = hub;
        _protoCacheRepository = protoCacheRepository;
    }

    public async Task Consume(ConsumeContext<IExampleEvent> context)
    {
        await _protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleCacheKey);

        await _hub.Clients.All.SendAsync(nameof(IExampleEvent), new
        {
            context.Message.CorrelationId,
            context.Message.Id,
        });
    }
}