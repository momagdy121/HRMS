using HRSystem.Business.Exceptions;
using HRSystem.Business.Helpers;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Business.Mapping;
using HRSystem.Common.Constants;
using HRSystem.Business.DTOs.UserAccounts;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace HRSystem.Business.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationUserRepository _applicationUsers;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        IApplicationUserRepository applicationUsers)
    {
        _userManager = userManager;
        _applicationUsers = applicationUsers;
    }

    public async Task CreateAccountAsync(int employeeId, string email, string password, string role, CancellationToken cancellationToken = default)
    {
        ValidateRole(role);

        var user = ApplicationUserMapper.FromDto(new CreateApplicationUserDto
        {
            EmployeeId = employeeId,
            Email = email
        });

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            throw new BusinessRuleException(
                string.Join("; ", createResult.Errors.Select(e => e.Description)));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            throw new BusinessRuleException(
                string.Join("; ", roleResult.Errors.Select(e => e.Description)));
        }
    }

    public Task<string> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default) =>
        throw new BusinessRuleException("Forgot password is not available yet. Coming soon.");

    public Task ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default) =>
        throw new BusinessRuleException("Password reset is not available yet. Coming soon.");

    public async Task ChangePasswordAsync(int userId, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _applicationUsers.GetByIdAsync(userId, cancellationToken)
                   ?? throw new NotFoundException("User account not found.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            throw new BusinessRuleException(
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        AccountLifecycle.MarkPasswordChanged(user);
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new BusinessRuleException(
                string.Join("; ", updateResult.Errors.Select(e => e.Description)));
        }
    }

    private static void ValidateRole(string role)
    {
        if (!RoleNames.AllRoles.Contains(role))
            throw new BusinessRuleException($"Invalid role '{role}'.");
    }
}
