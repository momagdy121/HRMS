namespace HRSystem.Business.Interfaces.Policies;

public interface IEmployeeDeletionPolicy
{
    Task<bool> CanDeleteAsync(int employeeId, CancellationToken cancellationToken = default);
}
