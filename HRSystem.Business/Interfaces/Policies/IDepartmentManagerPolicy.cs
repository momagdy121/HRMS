using HRSystem.Data.Models;

namespace HRSystem.Business.Interfaces.Policies;

public interface IDepartmentManagerPolicy
{
    bool IsValidManager(Employee employee, Department department);
}
