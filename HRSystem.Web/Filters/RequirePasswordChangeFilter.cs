using HRSystem.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HRSystem.Web.Filters;

public class RequirePasswordChangeFilter : IAsyncActionFilter
{
    private static readonly HashSet<string> ExemptActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "Login",
        "Logout",
        "ChangePassword",
        "ForgotPassword",
        "ResetPassword",
        "AccessDenied",
        "ComingSoon"
    };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;
        var user = httpContext.User;

        if (user.Identity?.IsAuthenticated == true
            && !IsExempt(context)
            && !await IsPasswordChangeSatisfiedAsync(httpContext))
        {
            context.Result = new RedirectToActionResult(
                "ChangePassword",
                "Account",
                new { area = string.Empty });
            return;
        }

        await next();
    }

    private static bool IsExempt(ActionExecutingContext context)
    {
        if (context.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller)
            && string.Equals(controller, "Account", StringComparison.OrdinalIgnoreCase)
            && context.ActionDescriptor.RouteValues.TryGetValue("action", out var action)
            && !string.IsNullOrEmpty(action)
            && ExemptActions.Contains(action))
        {
            return true;
        }

        return false;
    }

    private static async Task<bool> IsPasswordChangeSatisfiedAsync(HttpContext httpContext)
    {
        var userManager = httpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var applicationUser = await userManager.GetUserAsync(httpContext.User);
        return applicationUser is not { IsPasswordChangeRequired: true };
    }
}
