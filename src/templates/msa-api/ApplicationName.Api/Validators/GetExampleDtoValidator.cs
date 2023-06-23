using ApplicationName.Api.Contracts.Dtos;
using FluentValidation;

namespace ApplicationName.Api.Validators;

// ReSharper disable once ClassNeverInstantiated.Global
public class GetExampleDtoValidator : AbstractValidator<GetExampleDto>
{
    public GetExampleDtoValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}