using HRSystem.Business.Exceptions;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Data.Interfaces;
using EmployeeEntity = HRSystem.Data.Models.Employee;
using HRSystem.Data.Models;
using HRSystem.Web.Helpers;
using HRSystem.Web.ViewModels.UserAccounts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.HR.Controllers;

public class UserAccountController : HRBaseController
{
    private const int PageSize = 10;

    private readonly IUserAccountService _userAccountService;
    private readonly IAccountService _accountService;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserAccountController(
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IUserAccountService userAccountService,
        IAccountService accountService,
        UserManager<ApplicationUser> userManager)
        : base(currentUser, unitOfWork)
    {
        _userAccountService = userAccountService;
        _accountService = accountService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int? resetPasswordUserId = null)
    {
        await SetLayoutAsync("UserAccounts", "Search user accounts...");
        return View(await BuildIndexModelAsync(page, resetPasswordUserId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword([Bind(Prefix = "ResetPasswordForm")] ResetUserPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("UserAccounts", "Search user accounts...");
            return View("Index", await BuildIndexModelAsync(model.Page, resetPasswordUserId: model.UserId, resetPasswordForm: model));
        }

        try
        {
            await _accountService.ChangePasswordAsync(model.UserId, model.NewPassword);
            TempData["Success"] = "Password reset successfully.";
            return RedirectToAction(nameof(Index), new { page = model.Page });
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("UserAccounts", "Search user accounts...");
            ModalValidationHelper.AddFormErrors(ModelState, "ResetPasswordForm", ex.Message);
            return View("Index", await BuildIndexModelAsync(model.Page, resetPasswordUserId: model.UserId, resetPasswordForm: model));
        }
    }

    private async Task<HrUserAccountIndexViewModel> BuildIndexModelAsync(
        int page,
        int? resetPasswordUserId = null,
        ResetUserPasswordViewModel? resetPasswordForm = null)
    {
        var paged = await _userAccountService.GetAllAsync(page, PageSize);

        var items = paged.Items
            .Select(dto => new UserAccountListItemViewModel
            {
                UserId = dto.UserId,
                EmployeeId = dto.EmployeeId,
                FullName = dto.FullName,
                Initials = HrDisplayHelper.GetInitials(
                    dto.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "?",
                    dto.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).FirstOrDefault() ?? string.Empty),
                Email = dto.Email,
                Role = dto.Role,
                IsPasswordChangeRequired = dto.IsPasswordChangeRequired,
                IsActive = dto.IsActive,
                IsEmployeeDeleted = dto.IsEmployeeDeleted
            })
            .ToList();

        ResetUserPasswordViewModel? resolvedResetPassword = null;
        if (resetPasswordUserId.HasValue || resetPasswordForm != null)
        {
            var userId = resetPasswordForm?.UserId ?? resetPasswordUserId!.Value;
            var target = await ResolveAccountByUserIdAsync(userId, items);
            if (target != null)
            {
                resolvedResetPassword = resetPasswordForm ?? new ResetUserPasswordViewModel
                {
                    UserId = target.UserId,
                    FullName = target.FullName,
                    Page = page
                };
                if (resetPasswordForm != null)
                    resolvedResetPassword.Page = resetPasswordForm.Page;
            }
        }

        return new HrUserAccountIndexViewModel
        {
            Accounts = items,
            ResetPasswordForm = resolvedResetPassword,
            Page = paged.Page,
            TotalPages = paged.TotalPages,
            TotalCount = paged.TotalCount,
            PageSize = paged.PageSize
        };
    }

    private async Task<UserAccountListItemViewModel?> ResolveAccountByUserIdAsync(
        int userId,
        IReadOnlyList<UserAccountListItemViewModel> pageItems)
    {
        var fromPage = pageItems.FirstOrDefault(i => i.UserId == userId);
        if (fromPage != null)
            return fromPage;

        var user = await UnitOfWork.ApplicationUsers.GetByIdAsync(userId);
        if (user == null)
            return null;

        var employee = await UnitOfWork.Employees.GetByIdAsync(user.EmployeeId);
        if (employee == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;

        return MapToListItem(user, employee, role);
    }

    private static UserAccountListItemViewModel MapToListItem(ApplicationUser user, EmployeeEntity employee, string role) =>
        new()
        {
            UserId = user.Id,
            EmployeeId = employee.Id,
            FullName = $"{employee.FirstName} {employee.LastName}",
            Initials = HrDisplayHelper.GetInitials(employee.FirstName, employee.LastName),
            Email = user.Email ?? employee.Email,
            Role = role,
            IsPasswordChangeRequired = user.IsPasswordChangeRequired,
            IsActive = employee.IsActive,
            IsEmployeeDeleted = employee.IsDeleted
        };
}
