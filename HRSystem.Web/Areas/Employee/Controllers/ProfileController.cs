using HRSystem.Business.Interfaces.Services;
using HRSystem.Common.Enums;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using HRSystem.Web.Helpers;
using HRSystem.Web.ViewModels.Profile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.Employee.Controllers;

public class ProfileController : EmployeeBaseController
{
    private readonly ILeaveService _leaveService;
    private readonly UserManager<Data.Models.Employee> _userManager;
    private readonly ICloudinaryService _cloudinaryService;

    public ProfileController(
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        ILeaveService leaveService,
        UserManager<Data.Models.Employee> userManager,
        ICloudinaryService cloudinaryService)
        : base(currentUser, unitOfWork)
    {
        _leaveService = leaveService;
        _userManager = userManager;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await SetLayoutAsync("Profile", "Search...");
        var model = await BuildProfileViewModelAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateContact([Bind(Prefix = "UpdateContactForm")] UpdateProfileContactViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Profile", "Search...");
            var profileModel = await BuildProfileViewModelAsync();
            profileModel.UpdateContactForm = model;
            return View("Index", profileModel);
        }

        var userId = CurrentUser.GetCurrentUserId();
        var employee = await UnitOfWork.Employees.GetByIdAsync(userId);
        if (employee == null)
            return RedirectToAction(nameof(Index));

        employee.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();
        UnitOfWork.Employees.Update(employee);
        await UnitOfWork.SaveChangesAsync();

        TempData["Success"] = "Contact details updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAvatar(IFormFile? avatarFile)
    {
        if (avatarFile == null || avatarFile.Length == 0)
        {
            TempData["Error"] = "Please select an image file to upload.";
            return RedirectToAction(nameof(Index));
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
        {
            TempData["Error"] = "Only JPG, PNG, and WebP images are allowed.";
            return RedirectToAction(nameof(Index));
        }

        if (avatarFile.Length > 5 * 1024 * 1024)
        {
            TempData["Error"] = "Image file size cannot exceed 5MB.";
            return RedirectToAction(nameof(Index));
        }

        var userId = CurrentUser.GetCurrentUserId();
        var employee = await UnitOfWork.Employees.GetByIdAsync(userId);
        if (employee == null)
            return RedirectToAction(nameof(Index));

        var uploadedUrl = await _cloudinaryService.UploadImageAsync(avatarFile, "hrms/avatars");
        if (!string.IsNullOrEmpty(uploadedUrl))
        {
            employee.ProfilePictureUrl = uploadedUrl;
            UnitOfWork.Employees.Update(employee);
            await UnitOfWork.SaveChangesAsync();
            TempData["Success"] = "Profile photo updated successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to upload photo. Please try again.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<EmployeeProfileViewModel> BuildProfileViewModelAsync()
    {
        var employee = await CurrentUser.GetCurrentEmployeeAsync();
        var department = await UnitOfWork.Departments.GetByIdAsync(employee.DepartmentId);
        
        string managerName = "—";
        if (department != null && department.ManagerId > 0)
        {
            var manager = await UnitOfWork.Employees.GetByIdAsync(department.ManagerId);
            if (manager != null)
            {
                managerName = $"{manager.FirstName} {manager.LastName}";
            }
        }

        var roles = await _userManager.GetRolesAsync(employee);
        var role = roles.FirstOrDefault() ?? (employee.IsHR ? "HR" : "Employee");

        var currentYear = DateTime.UtcNow.Year;
        var annualBalance = await _leaveService.GetBalanceAsync(employee.Id, currentYear, LeaveType.Annual);
        var sickBalance = await _leaveService.GetBalanceAsync(employee.Id, currentYear, LeaveType.Sick);

        return new EmployeeProfileViewModel
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            FullName = $"{employee.FirstName} {employee.LastName}",
            Initials = HrDisplayHelper.GetInitials(employee.FirstName, employee.LastName),
            Email = employee.Email ?? string.Empty,
            PhoneNumber = employee.PhoneNumber,
            ProfilePictureUrl = employee.ProfilePictureUrl,
            Role = role,
            DepartmentName = department?.Name ?? "—",
            DepartmentManagerName = managerName,
            Salary = employee.Salary,
            HireDate = employee.HireDate,
            Tenure = CalculateTenure(employee.HireDate),
            IsActive = employee.IsActive,
            AnnualLeaveTotal = annualBalance?.TotalDays ?? 0,
            AnnualLeaveUsed = annualBalance?.UsedDays ?? 0,
            SickLeaveTotal = sickBalance?.TotalDays ?? 0,
            SickLeaveUsed = sickBalance?.UsedDays ?? 0,
            UpdateContactForm = new UpdateProfileContactViewModel
            {
                PhoneNumber = employee.PhoneNumber
            }
        };
    }

    private static string CalculateTenure(DateOnly hireDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (hireDate > today) return "New Joiner";

        var totalMonths = (today.Year - hireDate.Year) * 12 + today.Month - hireDate.Month;
        if (today.Day < hireDate.Day) totalMonths--;

        if (totalMonths <= 0)
        {
            var days = (today.ToDateTime(TimeOnly.MinValue) - hireDate.ToDateTime(TimeOnly.MinValue)).Days;
            return $"{days} day{(days == 1 ? "" : "s")}";
        }

        var years = totalMonths / 12;
        var months = totalMonths % 12;

        if (years > 0 && months > 0)
            return $"{years} yr{(years == 1 ? "" : "s")}, {months} mo{(months == 1 ? "" : "s")}";
        if (years > 0)
            return $"{years} year{(years == 1 ? "" : "s")}";

        return $"{months} month{(months == 1 ? "" : "s")}";
    }
}
