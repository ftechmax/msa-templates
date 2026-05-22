using ApplicationName.Api.Contracts.Dtos;
using FluentValidation;

namespace ApplicationName.Api.Validators;

public class ExampleValueObjectDtoValidator : AbstractValidator<ExampleValueObjectDto>
{
    public ExampleValueObjectDtoValidator()
    {
        RuleFor(i => i.Code).NotEmpty();
        RuleFor(i => i.Value).GreaterThan(0);
    }
}