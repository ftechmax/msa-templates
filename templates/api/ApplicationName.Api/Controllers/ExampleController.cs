using ApplicationName.Api.Application.Services;
using ApplicationName.Api.Contracts.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationName.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ExampleController(IExampleService applicationService) : ControllerBase
{
    [HttpGet]
    public Task<IEnumerable<ExampleCollectionDto>> GetCollection()
    {
        return applicationService.GetCollectionAsync();
    }

    [HttpGet("{id}")]
    public Task<ExampleDetailsDto?> Get(Guid id)
    {
        return applicationService.GetAsync(id);
    }

    [HttpPost]
    public Task Post([FromBody] CreateExampleDto dto)
    {
        return applicationService.HandleAsync(dto);
    }

    [HttpPut("{id}")]
    public Task Put(Guid id, [FromBody] UpdateExampleDto dto)
    {
        return applicationService.HandleAsync(id, dto);
    }
}