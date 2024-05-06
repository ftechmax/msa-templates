using ApplicationName.Api.Contracts.Dtos;
using FluentValidation;

namespace ApplicationName.Api.Validators;

public class CreateExampleDtoValidator : AbstractValidator<CreateExampleDto>
{
    public CreateExampleDtoValidator()
    {
        RuleFor(i => i.CorrelationId).NotEmpty();
        RuleFor(i => i.Name).NotEmpty();
        RuleFor(i => i.Description).NotEmpty();
        RuleFor(i => i.ExampleValueObject).NotNull();
    }
}
