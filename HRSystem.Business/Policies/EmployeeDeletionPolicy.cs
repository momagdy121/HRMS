using HRSystem.Business.Interfaces.Policies;
using HRSystem.Data.Interfaces;

namespace HRSystem.Business.Policies;

public class EmployeeDeletionPolicy : IEmployeeDeletionPolicy
{
    private readonly IEmployeeRepository _employees;

    public EmployeeDeletionPolicy(IEmployeeRepository employees)
    {
        _employees = employees;
    }

    public async Task<bool> CanDeleteAsync(int employeeId, CancellationToken cancellationToken = default) =>
        !await _employees.IsManagerOfAnyDepartmentAsync(employeeId, cancellationToken);
}
