namespace HRSystem.Business.Interfaces.Services;

public interface IAccountService
{
    Task CreateAccountAsync(int employeeId, string email, string password, string role, CancellationToken cancellationToken = default);

    Task<string> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);

    Task ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);

    Task ChangeRoleAsync(int employeeId, string newRole, CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(int userId, string newPassword, CancellationToken cancellationToken = default);
}
