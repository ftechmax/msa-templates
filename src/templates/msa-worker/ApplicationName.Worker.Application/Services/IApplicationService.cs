using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Contracts.Commands;
using System.Threading.Tasks;

namespace ApplicationName.Worker.Application.Services;

public interface IApplicationService
{
    Task<ExampleDocument> HandleAsync(IExampleCommand command);
}