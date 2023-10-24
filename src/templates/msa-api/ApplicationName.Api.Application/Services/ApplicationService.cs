using System;
using System.Threading.Tasks;
using ApplicationName.Api.Application.Commands;
using ApplicationName.Api.Application.Documents;
using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Contracts;
using ApplicationName.Api.Contracts.Commands;
using ApplicationName.Api.Contracts.Dtos;
using ArgDefender;
using AutoMapper;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;

namespace ApplicationName.Api.Application.Services;

public sealed class ApplicationService : IApplicationService
{
    private readonly IDocumentRepository _documentRepository;

    private readonly IProtoCacheRepository _protoCacheRepository;

    private readonly IMapper _mapper;

    private readonly ISendEndpoint _sendEndpoint;

    public ApplicationService(
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

    public async Task<ExampleResultDto> GetAsync(GetExampleDto dto)
    {
        Guard.Argument(dto).NotNull();
        Guard.Argument(dto.Id).NotDefault();

        var key = GetKey(dto.Id);
        var result = await _protoCacheRepository.GetAsync<ExampleResultDto>(key);
        if (result != default)
        {
            return result;
        }

        var document = await _documentRepository.GetAsync<ExampleDocument>(dto.Id);
        if (document == default)
        {
            return default;
        }

        result = _mapper.Map<ExampleResultDto>(document);

        await _protoCacheRepository.SetAsync(key, result, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(1) });

        return result;
    }

    public Task HandleAsync(CreateExampleDto dto)
    {
        var command = _mapper.Map<ExampleCommand>(dto);
        return _sendEndpoint.Send<IExampleCommand>(command);
    }

    private static string GetKey(Guid id)
    {
        return $"{ApplicationConstants.ExampleCacheKey}_{id:D}";
    }
}