using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ApplicationName.Api.Validators;

public static class ValidationResultExtensions
{
    public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState)
    {
        foreach (var failure in result.Errors)
        {
            modelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
        }
    }
}
