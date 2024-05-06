using ApplicationName.Api.Contracts.Dtos;
using FluentValidation;

namespace ApplicationName.Api.Validators;

public class AddExampleEntityDtoValidator : AbstractValidator<AddExampleEntityDto>
{
    public AddExampleEntityDtoValidator()
    {
        RuleFor(i => i.CorrelationId).NotEmpty();
        RuleFor(i => i.Id).NotEmpty();
        RuleFor(i => i.Name).NotEmpty();
        RuleFor(i => i.SomeValue).GreaterThan(0);
    }
}
