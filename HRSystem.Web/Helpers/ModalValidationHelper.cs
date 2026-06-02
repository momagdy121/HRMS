using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HRSystem.Web.Helpers;

public static class ModalValidationHelper
{
    public static void AddFormErrors(ModelStateDictionary modelState, string fieldPrefix, string message)
    {
        foreach (var part in message.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            modelState.AddModelError(fieldPrefix, part);
        }
    }
}
