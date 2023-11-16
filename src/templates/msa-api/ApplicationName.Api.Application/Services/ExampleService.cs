using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationName.Api.Application.Commands;
using ApplicationName.Api.Application.Documents;
using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Contracts;
using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Worker.Contracts.Commands;
using ArgDefender;
using AutoMapper;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;

namespace ApplicationName.Api.Application.Services;

public sealed class ExampleService : IExampleService
{
    private readonly IDocumentRepository _documentRepository;

    private readonly IProtoCacheRepository _protoCacheRepository;

    private readonly IMapper _mapper;

    private readonly ISendEndpoint _sendEndpoint;

    public ExampleService(
        IDocumentRepository documentRepository,
        IProtoCacheRepository protoCacheRepository,
        IMapper mapper,
        ISendEndpoint sendEndpoint)
    {
        _documentRepository = documentRepository;
        _protoCacheRepository = protoCacheRepository;
        _mapper = mapper;
        _sendEndpoint = sendEndpoint;
    }

    public async Task<IEnumerable<ExampleCollectionDto>> GetCollectionAsync()
    {
        var result = await _protoCacheRepository.GetAsync<IEnumerable<ExampleCollectionDto>>(ApplicationConstants.ExampleCollectionCacheKey);
        if (result != default)
        {
            return result;
        }

        var documents = await _documentRepository.GetAllAsync<ExampleDocument>(_ => true);
        if (!documents.Any())
        {
            return new List<ExampleCollectionDto>();
        }

        result = _mapper.Map<IEnumerable<ExampleCollectionDto>>(documents);

        await _protoCacheRepository.SetAsync(ApplicationConstants.ExampleCollectionCacheKey, result, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(1) });

        return result;
    }

    public async Task<ExampleDetailsDto> GetAsync(Guid id)
    {
        Guard.Argument(id).NotDefault();

        var key = $"{ApplicationConstants.ExampleDetailsCacheKey}_{id:N}";

        var result = await _protoCacheRepository.GetAsync<ExampleDetailsDto>(key);
        if (result != default)
        {
            return result;
        }

        var document = await _documentRepository.GetAsync<ExampleDocument>(i => i.Id == id);
        if (document == default)
        {
            return default;
        }

        result = _mapper.Map<ExampleDetailsDto>(document);

        await _protoCacheRepository.SetAsync(key, result, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(1) });

        return result;
    }

    public Task HandleAsync(CreateExampleDto dto)
    {
        Guard.Argument(dto).NotNull();

        var command = _mapper.Map<CreateExampleCommand>(dto);
        return _sendEndpoint.Send<ICreateExampleCommand>(command);
    }

    public Task HandleAsync(Guid id, UpdateExampleDto dto)
    {
        Guard.Argument(id).NotDefault();
        Guard.Argument(dto).NotNull();

        var command = _mapper.Map<UpdateExampleCommand>(dto);
        return _sendEndpoint.Send<IUpdateExampleCommand>(command);
    }
}