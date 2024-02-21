using ApplicationName.Api.Contracts.Dtos;
using FluentValidation;

namespace ApplicationName.Api.Validators;

// ReSharper disable once ClassNeverInstantiated.Global
public class CreateExampleDtoValidator : AbstractValidator<CreateExampleDto>
{
    public CreateExampleDtoValidator()
    {
        RuleFor(i => i.Name).NotEmpty();
    }
}