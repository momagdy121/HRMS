using HRSystem.Data.Models;

namespace HRSystem.Business.Interfaces.Services;

public interface ICurrentUserService
{
    int GetCurrentUserId();

    bool IsHR();

    bool IsDepartmentHead();

    string? GetFullName();

    string? GetDepartmentName();

    int? GetDepartmentId();

    Task<Employee> GetCurrentEmployeeAsync(CancellationToken cancellationToken = default);
}
