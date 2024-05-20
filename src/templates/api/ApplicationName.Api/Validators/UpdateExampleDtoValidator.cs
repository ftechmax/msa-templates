using ApplicationName.Api.Contracts.Dtos;
using FluentValidation;

namespace ApplicationName.Api.Validators;

public class UpdateExampleDtoValidator : AbstractValidator<UpdateExampleDto>
{
    public UpdateExampleDtoValidator()
    {
        RuleFor(i => i.CorrelationId).NotEmpty();
        RuleFor(i => i.Description).NotEmpty();
        RuleFor(i => i.ExampleValueObject).NotNull().SetValidator(new ExampleValueObjectDtoValidator());
    }
}