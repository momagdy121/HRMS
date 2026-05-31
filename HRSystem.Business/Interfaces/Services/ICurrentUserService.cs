using HRSystem.Data.Models;

namespace HRSystem.Business.Interfaces.Services;

public interface ICurrentUserService
{
    int GetCurrentUserId();

    bool IsHR();

    bool IsDepartmentHead();

    Task<Employee> GetCurrentEmployeeAsync(CancellationToken cancellationToken = default);
}
