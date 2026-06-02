using HRSystem.Business.Interfaces.Policies;
using HRSystem.Data.Interfaces;

namespace HRSystem.Business.Policies;

public class DepartmentDeletionPolicy : IDepartmentDeletionPolicy
{
    private readonly IDepartmentRepository _departments;

    public DepartmentDeletionPolicy(IDepartmentRepository departments)
    {
        _departments = departments;
    }

    public async Task<bool> CanDeleteAsync(int departmentId, CancellationToken cancellationToken = default) =>
        await _departments.CountActiveEmployeesAsync(departmentId, cancellationToken) == 0;
}