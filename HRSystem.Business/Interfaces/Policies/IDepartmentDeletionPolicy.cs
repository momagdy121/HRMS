namespace HRSystem.Business.Interfaces.Policies;

public interface IDepartmentDeletionPolicy
{
    Task<bool> CanDeleteAsync(int departmentId, CancellationToken cancellationToken = default);
}
