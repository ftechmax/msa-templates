using ApplicationName.Api.Contracts.Dtos;

namespace ApplicationName.Api.Application.Services;

public interface IExampleService
{
    Task<IEnumerable<ExampleCollectionDto>> GetCollectionAsync();

    Task<ExampleDetailsDto> GetAsync(Guid id);

    Task HandleAsync(CreateExampleDto dto);

    Task HandleAsync(Guid id, UpdateExampleDto dto);
}