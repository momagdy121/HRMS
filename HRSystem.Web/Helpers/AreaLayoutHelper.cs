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
        var employee = await currentUser.GetCurrentEmployeeAsync();
        var department = await unitOfWork.Departments.GetByIdAsync(employee.DepartmentId);

        controller.ViewBag.Role = layoutRole;
        controller.ViewBag.ActivePage = activePage;
        controller.ViewBag.UserName = $"{employee.FirstName} {employee.LastName}";
        controller.ViewBag.UserTitle = BuildUserTitle(currentUser, department?.Name);
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
