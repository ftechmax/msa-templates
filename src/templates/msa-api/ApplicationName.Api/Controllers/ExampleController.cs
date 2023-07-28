using ApplicationName.Api.Application.Services;
using ApplicationName.Api.Contracts.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ApplicationName.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ExampleController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public ExampleController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpGet]
    public Task<ExampleResultDto> Get([FromQuery] GetExampleDto dto)
    {
        return _applicationService.GetAsync(dto);
    }

    [HttpPost]
    public Task Post([FromBody] CreateExampleDto dto)
    {
        return _applicationService.HandleAsync(dto);
    }
}