using HRSystem.Data.Models;

namespace HRSystem.Business.Interfaces.Services;

public interface IAccountService
{
    Task CreateAccountAsync(Employee employee, string password, string role, CancellationToken cancellationToken = default);

    Task<bool> ForgotPasswordAsync(string email, string resetCallbackUrl, CancellationToken cancellationToken = default);

    Task ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(int userId, string newPassword, CancellationToken cancellationToken = default);
}
