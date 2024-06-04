using ApplicationName.Api.Application.Documents;
using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Contracts;
using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Shared.Commands;
using ArgDefender;
using AutoMapper;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;

namespace ApplicationName.Api.Application.Services;

public sealed class ExampleService(
    IDocumentRepository documentRepository,
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

        var documents = await documentRepository.GetAllAsync<ExampleDocument>(_ => true);
        if (!documents.Any())
        {
            return [];
        }

        result = mapper.Map<IEnumerable<ExampleCollectionDto>>(documents);

        await protoCacheRepository.SetAsync(ApplicationConstants.ExampleCollectionCacheKey, result, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(1) });

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

        var document = await documentRepository.GetAsync<ExampleDocument>(i => i.Id == id);
        if (document == default)
        {
            return default;
        }

        result = mapper.Map<ExampleDetailsDto>(document);

        await protoCacheRepository.SetAsync(key, result, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(1) });

        return result;
    }

    public async Task HandleAsync(CreateExampleDto dto)
    {
        Guard.Argument(dto).NotNull();

        var command = mapper.Map<CreateExampleCommand>(dto);
        var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(ApplicationConstants.MessageEndpoint);
        await sendEndpoint.Send(command);
    }

    public async Task HandleAsync(Guid id, UpdateExampleDto dto)
    {
        Guard.Argument(id).NotDefault();
        Guard.Argument(dto).NotNull();

        var command = mapper.Map<UpdateExampleCommand>(dto) with { Id = id };
        var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(ApplicationConstants.MessageEndpoint);
        await sendEndpoint.Send(command);
    }
}