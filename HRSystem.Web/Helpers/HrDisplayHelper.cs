using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;

namespace HRSystem.Web.Helpers;

public static class HrDisplayHelper
{
    public static string GetInitials(string firstName, string lastName)
    {
        var first = string.IsNullOrWhiteSpace(firstName) ? "?" : firstName[0].ToString();
        var last = string.IsNullOrWhiteSpace(lastName) ? string.Empty : lastName[0].ToString();
        return (first + last).ToUpperInvariant();
    }

    public static async Task<IReadOnlyDictionary<int, Department>> LoadDepartmentsAsync(
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var page = await unitOfWork.Departments.GetActivePagedAsync(1, 500, cancellationToken);
        return page.Items.ToDictionary(d => d.Id);
    }

    public static async Task<IReadOnlyDictionary<int, Employee>> LoadEmployeesAsync(
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var page = await unitOfWork.Employees.GetActivePagedAsync(1, 500, cancellationToken);
        return page.Items.ToDictionary(e => e.Id);
    }
}
