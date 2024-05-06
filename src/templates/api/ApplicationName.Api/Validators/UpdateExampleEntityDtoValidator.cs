using ApplicationName.Api.Contracts.Dtos;
using FluentValidation;

namespace ApplicationName.Api.Validators;

public class UpdateExampleEntityDtoValidator : AbstractValidator<UpdateExampleEntityDto>
{
    public UpdateExampleEntityDtoValidator()
    {
        RuleFor(i => i.CorrelationId).NotEmpty();
        RuleFor(i => i.Id).NotEmpty();
        RuleFor(i => i.EntityId).NotEmpty();
        RuleFor(i => i.SomeValue).GreaterThan(0);
    }
}
