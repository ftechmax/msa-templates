using ApplicationName.Api.Contracts.Dtos;
using System.Threading.Tasks;

namespace ApplicationName.Api.Application.Services;

public interface IApplicationService
{
    Task<ExampleResultDto> GetAsync(GetExampleDto dto);

    Task HandleAsync(CreateExampleDto dto);
}