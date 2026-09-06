using ApplicationName.Api.Application.Services;
using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Api.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationName.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ExampleController(
    IExampleService applicationService,
    IValidator<CreateExampleDto> createExampleDtoValidator,
    IValidator<UpdateExampleDto> updateExampleDtoValidator) : ControllerBase
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
    public async Task<IActionResult> Post([FromBody] CreateExampleDto dto)
    {
        var result = await createExampleDtoValidator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            result.AddToModelState(ModelState);
            return ValidationProblem(ModelState);
        }

        await applicationService.HandleAsync(dto);

        return Accepted();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(Guid id, [FromBody] UpdateExampleDto dto)
    {
        var result = await updateExampleDtoValidator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            result.AddToModelState(ModelState);
            return ValidationProblem(ModelState);
        }

        await applicationService.HandleAsync(id, dto);

        return Accepted();
    }
}
