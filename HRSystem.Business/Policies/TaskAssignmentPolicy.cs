using HRSystem.Business.Interfaces.Policies;
using HRSystem.Data.Models;

namespace HRSystem.Business.Policies;

public class TaskAssignmentPolicy : ITaskAssignmentPolicy
{
    public bool CanAssign(Employee assigner, Employee assignee, int departmentId) =>
        assignee.DepartmentId == departmentId
        && assigner.DepartmentId == departmentId
        && !assignee.IsHR
        && assigner.Id != assignee.Id
        && assigner.IsActive
        && !assigner.IsDeleted
        && assignee.IsActive
        && !assignee.IsDeleted;
}
