using HRSystem.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Helpers;

public static class AuthRedirectHelper
{
    public static IActionResult ToRoleDashboard(Controller controller)
    {
        if (controller.User.IsInRole(RoleNames.HR))
        {
            return controller.RedirectToAction("Index", "Dashboard", new { area = "HR" });
        }

        if (controller.User.IsInRole(RoleNames.DepartmentHead))
        {
            return controller.RedirectToAction("Index", "Dashboard", new { area = "DepartmentHead" });
        }

        return controller.RedirectToAction("Index", "Dashboard", new { area = "Employee" });
    }
}
