using ApplicationName.Api.Application.Commands;
using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Contracts;
using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Projections;
using ArgDefender;
using AutoMapper;
using MassTransit;

namespace ApplicationName.Api.Application.Services;

public sealed class ExampleService(
    IProtoCacheRepository protoCacheRepository,
    IMapper mapper,
    ISendEndpointProvider sendEndpointProvider)
    : IExampleService
{
    public async Task<IEnumerable<ExampleCollectionDto>> GetCollectionAsync()
    {
        var result = await protoCacheRepository.GetAsync<IEnumerable<ExampleCollectionDto>>(ApplicationConstants.ExampleCollectionCacheKey);
        if (result != default)
        {
            return result;
        }

        var projections = await protoCacheRepository.GetAllAsync<ExampleProjection>(nameof(ExampleProjection));
        if (!projections.Any())
        {
            return [];
        }

        result = mapper.Map<IEnumerable<ExampleCollectionDto>>(projections);

        await protoCacheRepository.SetAsync(ApplicationConstants.ExampleCollectionCacheKey, result, TimeSpan.FromDays(1));

        return result;
    }

    public async Task<ExampleDetailsDto> GetAsync(Guid id)
    {
        Guard.Argument(id).NotDefault();

        var key = ApplicationConstants.ExampleDetailsCacheKey(id);

        var result = await protoCacheRepository.GetAsync<ExampleDetailsDto>(key);
        if (result != default)
        {
            return result;
        }

        var projection = await protoCacheRepository.GetAsync<ExampleProjection>($"{nameof(ExampleProjection)}:{id:N}");
        if (projection == default)
        {
            return default;
        }

        result = mapper.Map<ExampleDetailsDto>(projection);

        await protoCacheRepository.SetAsync(key, result, TimeSpan.FromDays(1));

        return result;
    }

    public async Task HandleAsync(CreateExampleDto dto)
    {
        Guard.Argument(dto).NotNull();

        var command = mapper.Map<CreateExampleCommand>(dto);
        var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(ApplicationConstants.MessageEndpoint);
        await sendEndpoint.Send<ICreateExampleCommand>(command);
    }

    public async Task HandleAsync(Guid id, UpdateExampleDto dto)
    {
        Guard.Argument(id).NotDefault();
        Guard.Argument(dto).NotNull();

        var command = mapper.Map<UpdateExampleCommand>(dto);
        command.Id = id;
        var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(ApplicationConstants.MessageEndpoint);
        await sendEndpoint.Send<IUpdateExampleCommand>(command);
    }

    public async Task HandleAsync(Guid id, AddExampleEntityDto dto)
    {
        Guard.Argument(id).NotDefault();
        Guard.Argument(dto).NotNull();

        var command = mapper.Map<AddExampleEntityCommand>(dto);
        command.Id = id;
        var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(ApplicationConstants.MessageEndpoint);
        await sendEndpoint.Send<IAddExampleEntityCommand>(command);
    }

    public async Task HandleAsync(Guid id, Guid entityId, UpdateExampleEntityDto dto)
    {
        Guard.Argument(id).NotDefault();
        Guard.Argument(entityId).NotDefault();
        Guard.Argument(dto).NotNull();

        var command = mapper.Map<UpdateExampleEntityCommand>(dto);
        command.Id = id;
        command.EntityId = entityId;
        var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(ApplicationConstants.MessageEndpoint);
        await sendEndpoint.Send<IUpdateExampleEntityCommand>(command);
    }
}