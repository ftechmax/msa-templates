using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Contracts;
using ApplicationName.Api.Contracts.Events;
using MassTransit;
using System.Threading.Tasks;

namespace ApplicationName.Api.Consumers;

public class LocalEventHandler : IConsumer<IExampleEvent>
{
    private readonly IProtoCacheRepository _protoCacheRepository;

    public LocalEventHandler(IProtoCacheRepository protoCacheRepository)
    {
        _protoCacheRepository = protoCacheRepository;
    }

    public async Task Consume(ConsumeContext<IExampleEvent> context)
    {
        await _protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleCacheKey);
    }
}