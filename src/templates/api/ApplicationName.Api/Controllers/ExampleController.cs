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
    public Task<ExampleDetailsDto> Get(Guid id)
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

    [HttpPost("{id}/entities")]
    public Task Put(Guid id, [FromBody] AddExampleEntityDto dto)
    {
        return applicationService.HandleAsync(id, dto);
    }

    [HttpPut("{id}/entities/{entityId}")]
    public Task Put(Guid id, Guid entityId, [FromBody] UpdateExampleEntityDto dto)
    {
        return applicationService.HandleAsync(id, entityId, dto);
    }
}