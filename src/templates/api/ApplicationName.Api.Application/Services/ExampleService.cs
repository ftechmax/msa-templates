﻿using ApplicationName.Api.Application.Commands;
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
    ISendEndpoint sendEndpoint)
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
            return new List<ExampleCollectionDto>();
        }

        result = mapper.Map<IEnumerable<ExampleCollectionDto>>(documents);

        await protoCacheRepository.SetAsync(ApplicationConstants.ExampleCollectionCacheKey, result, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(1) });

        return result;
    }

    public async Task<ExampleDetailsDto> GetAsync(Guid id)
    {
        Guard.Argument(id).NotDefault();

        var key = $"{ApplicationConstants.ExampleDetailsCacheKey}_{id:N}";

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

    public Task HandleAsync(CreateExampleDto dto)
    {
        Guard.Argument(dto).NotNull();

        var command = mapper.Map<CreateExampleCommand>(dto);
        return sendEndpoint.Send<ICreateExampleCommand>(command);
    }

    public Task HandleAsync(Guid id, UpdateExampleDto dto)
    {
        Guard.Argument(id).NotDefault();
        Guard.Argument(dto).NotNull();

        var command = mapper.Map<UpdateExampleCommand>(dto);
        return sendEndpoint.Send<IUpdateExampleCommand>(command);
    }

    public Task HandleAsync(Guid id, AddExampleEntityDto dto)
    {
        Guard.Argument(id).NotDefault();
        Guard.Argument(dto).NotNull();

        var command = mapper.Map<AddExampleEntityCommand>(dto);
        return sendEndpoint.Send<IAddExampleEntityCommand>(command);
    }

    public Task HandleAsync(Guid id, Guid entityId, UpdateExampleEntityDto dto)
    {
        Guard.Argument(id).NotDefault();
        Guard.Argument(entityId).NotDefault();
        Guard.Argument(dto).NotNull();

        var command = mapper.Map<UpdateExampleEntityCommand>(dto);
        return sendEndpoint.Send<IUpdateExampleEntityCommand>(command);
    }
}