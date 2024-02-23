using ApplicationName.Api.Contracts.Dtos;
using FluentValidation;

namespace ApplicationName.Api.Validators;

public class UpdateExampleEntityDtoValidator : AbstractValidator<UpdateExampleEntityDto>
{
    public UpdateExampleEntityDtoValidator()
    {
        RuleFor(i => i.Name).NotEmpty();
    }
}