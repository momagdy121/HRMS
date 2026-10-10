using HRSystem.Business.Exceptions;
using HRSystem.Business.Helpers;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Common.Constants;
using HRSystem.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace HRSystem.Business.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<Employee> _userManager;
    private readonly IEmailService _emailService;

    public AccountService(UserManager<Employee> userManager, IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task CreateAccountAsync(Employee employee, string password, string role, CancellationToken cancellationToken = default)
    {
        ValidateRole(role);

        var createResult = await _userManager.CreateAsync(employee, password);
        if (!createResult.Succeeded)
        {
            throw new BusinessRuleException(
                string.Join("; ", createResult.Errors.Select(e => e.Description)));
        }

        var roleResult = await _userManager.AddToRoleAsync(employee, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(employee);
            throw new BusinessRuleException(
                string.Join("; ", roleResult.Errors.Select(e => e.Description)));
        }
    }

    public async Task<bool> ForgotPasswordAsync(string email, string resetCallbackUrl, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim();
        var employee = await _userManager.FindByEmailAsync(normalized);
        if (employee is null)
            return false;

        var token = await _userManager.GeneratePasswordResetTokenAsync(employee);

        var resetUrl = resetCallbackUrl
            .Replace("__EMAIL__", Uri.EscapeDataString(normalized))
            .Replace("__TOKEN__", Uri.EscapeDataString(token));

        var employeeName = $"{employee.FirstName} {employee.LastName}".Trim();
        if (string.IsNullOrEmpty(employeeName))
            employeeName = normalized;

        var htmlContent = EmailTemplates.PasswordResetEmail(employeeName, resetUrl);

        await _emailService.SendEmailAsync(
            normalized,
            employeeName,
            "Password Reset - HRMS Portal",
            htmlContent,
            cancellationToken);

        return true;
    }

    public async Task ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var employee = await _userManager.FindByEmailAsync(email.Trim())
                       ?? throw new BusinessRuleException("Invalid reset request.");

        var result = await _userManager.ResetPasswordAsync(employee, token, newPassword);
        if (!result.Succeeded)
        {
            throw new BusinessRuleException(
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        AccountLifecycle.MarkPasswordChanged(employee);
        var updateResult = await _userManager.UpdateAsync(employee);
        if (!updateResult.Succeeded)
        {
            throw new BusinessRuleException(
                string.Join("; ", updateResult.Errors.Select(e => e.Description)));
        }
    }

    public async Task ChangePasswordAsync(int userId, string newPassword, CancellationToken cancellationToken = default)
    {
        var employee = await _userManager.FindByIdAsync(userId.ToString())
                       ?? throw new NotFoundException("User account not found.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(employee);
        var result = await _userManager.ResetPasswordAsync(employee, token, newPassword);
        if (!result.Succeeded)
        {
            throw new BusinessRuleException(
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        AccountLifecycle.MarkPasswordChanged(employee);
        var updateResult = await _userManager.UpdateAsync(employee);
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
