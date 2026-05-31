using HRSystem.Business.Interfaces.Policies;
using HRSystem.Data.Models;

namespace HRSystem.Business.Policies;

public class DepartmentManagerPolicy : IDepartmentManagerPolicy
{
    public bool IsValidManager(Employee employee, Department department) =>
        !employee.IsHR
        && employee.DepartmentId == department.Id
        && employee.IsActive
        && !employee.IsDeleted;
}
