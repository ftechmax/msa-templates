using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationName.Api.Application.Services;
using ApplicationName.Api.Contracts.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationName.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ExampleController : ControllerBase
{
    private readonly IExampleService _applicationService;

    public ExampleController(IExampleService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpGet]
    public Task<IEnumerable<ExampleCollectionDto>> GetCollection()
    {
        return _applicationService.GetCollectionAsync();
    }

    [HttpGet("{id}")]
    public Task<ExampleDetailsDto> Get(Guid id)
    {
        return _applicationService.GetAsync(id);
    }

    [HttpPost]
    public Task Post([FromBody] CreateExampleDto dto)
    {
        return _applicationService.HandleAsync(dto);
    }

    [HttpPut("{id}")]
    public Task Put(Guid id, [FromBody] UpdateExampleDto dto)
    {
        return _applicationService.HandleAsync(id, dto);
    }
}