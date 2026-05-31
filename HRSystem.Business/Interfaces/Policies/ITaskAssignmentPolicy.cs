using HRSystem.Data.Models;

namespace HRSystem.Business.Interfaces.Policies;

public interface ITaskAssignmentPolicy
{
    bool CanAssign(Employee assigner, Employee assignee);
}
