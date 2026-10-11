using HRSystem.Business.Interfaces.Services;
using HRSystem.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Helpers;

public static class AreaLayoutHelper
{
    public static async Task SetAsync(
        Controller controller,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        string layoutRole,
        string activePage,
        string? searchPlaceholder = null)
    {
        string effectiveRole = "Employee";
        if (currentUser.IsHR())
            effectiveRole = "HR";
        else if (currentUser.IsDepartmentHead())
            effectiveRole = "DeptHead";

        var fullName = currentUser.GetFullName();
        var departmentName = currentUser.GetDepartmentName();

        string? avatarUrl = null;
        try
        {
            var employee = await currentUser.GetCurrentEmployeeAsync();
            avatarUrl = employee.ProfilePictureUrl;
            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = $"{employee.FirstName} {employee.LastName}".Trim();
                var department = await unitOfWork.Departments.GetByIdAsync(employee.DepartmentId);
                departmentName = department?.Name;
            }
        }
        catch
        {
            // Fallback
        }

        controller.ViewBag.Role = effectiveRole;
        controller.ViewBag.ActivePage = activePage;
        controller.ViewBag.UserName = fullName;
        controller.ViewBag.UserTitle = BuildUserTitle(currentUser, departmentName);
        controller.ViewBag.UserAvatar = avatarUrl;
        controller.ViewBag.SearchPlaceholder = searchPlaceholder ?? "Search...";
    }

    private static string BuildUserTitle(ICurrentUserService currentUser, string? departmentName)
    {
        if (currentUser.IsHR())
            return "HR";

        if (currentUser.IsDepartmentHead())
            return string.IsNullOrEmpty(departmentName) ? "Department Head" : $"Department Head — {departmentName}";

        return string.IsNullOrEmpty(departmentName) ? "Employee" : $"Employee — {departmentName}";
    }
}
