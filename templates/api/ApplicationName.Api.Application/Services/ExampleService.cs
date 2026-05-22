using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Contracts;
using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Projections;
using ArgDefender;
using MapsterMapper;
using MassTransit;

namespace ApplicationName.Api.Application.Services;

public sealed class ExampleService(
    IProtoCacheRepository protoCacheRepository,
    IMapper mapper,
    IBus bus)
    : IExampleService
{
    public async Task<IEnumerable<ExampleCollectionDto>> GetCollectionAsync()
    {
        var result = await protoCacheRepository.GetAsync<IEnumerable<ExampleCollectionDto>>(ApplicationConstants.ExampleCollectionCacheKey);
        if (result != null)
        {
            return result;
        }

        var projections = await protoCacheRepository.GetAllAsync<ExampleProjection>(ApplicationConstants.ExampleProjectionCacheKey);
        if (!projections.Any())
        {
            return [];
        }

        result = mapper.Map<IEnumerable<ExampleCollectionDto>>(projections);

        await protoCacheRepository.SetAsync(ApplicationConstants.ExampleCollectionCacheKey, result, TimeSpan.FromDays(1));

        return result;
    }

    public async Task<ExampleDetailsDto?> GetAsync(Guid id)
    {
        Guard.Argument(id).NotDefault();

        var key = ApplicationConstants.ExampleDetailsCacheKey(id);

        var result = await protoCacheRepository.GetAsync<ExampleDetailsDto>(key);
        if (result != null)
        {
            return result;
        }

        var projection = await protoCacheRepository.GetAsync<ExampleProjection>(ApplicationConstants.ExampleProjectionByIdCacheKey(id));
        if (projection == null)
        {
            return null;
        }

        result = mapper.Map<ExampleDetailsDto>(projection);

        await protoCacheRepository.SetAsync(key, result, TimeSpan.FromDays(1));

        return result;
    }

    public async Task HandleAsync(CreateExampleDto dto)
    {
        Guard.Argument(dto).NotNull();

        var command = mapper.Map<CreateExampleCommand>(dto);
        await bus.Send(command);
    }

    public async Task HandleAsync(Guid id, UpdateExampleDto dto)
    {
        Guard.Argument(id).NotDefault();
        Guard.Argument(dto).NotNull();

        var command = mapper.Map<UpdateExampleCommand>(dto) with { Id = id };
        await bus.Send(command);
    }
}