using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OpenCCG.Data.Extensions;

public static class ValidationResultExtensions
{
    public static void AddToModelState(this ValidationResult validationResult,
        ModelStateDictionary modelStateDictionary)
    {
        foreach (var error in validationResult.Errors)
        {
            modelStateDictionary.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
}