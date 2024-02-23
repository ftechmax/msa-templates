using ApplicationName.Api.Contracts.Dtos;
using FluentValidation;

namespace ApplicationName.Api.Validators;

public class AddExampleEntityDtoValidator : AbstractValidator<AddExampleEntityDto>
{
    public AddExampleEntityDtoValidator()
    {
        RuleFor(i => i.Name).NotEmpty();
    }
}