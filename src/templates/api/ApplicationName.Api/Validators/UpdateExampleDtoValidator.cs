using ApplicationName.Api.Contracts.Dtos;
using FluentValidation;

namespace ApplicationName.Api.Validators;

public class UpdateExampleDtoValidator : AbstractValidator<UpdateExampleDto>
{
    public UpdateExampleDtoValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}