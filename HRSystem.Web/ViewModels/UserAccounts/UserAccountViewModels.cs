using System.ComponentModel.DataAnnotations;

namespace HRSystem.Web.ViewModels.UserAccounts;

public class UserAccountListItemViewModel
{
    public int UserId { get; set; }
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsPasswordChangeRequired { get; set; }
    public bool IsActive { get; set; }
    public bool IsEmployeeDeleted { get; set; }
}

public class ResetUserPasswordViewModel
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Page { get; set; } = 1;

    [Required]
    [MinLength(8)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class HrUserAccountIndexViewModel
{
    public IReadOnlyList<UserAccountListItemViewModel> Accounts { get; set; } = [];
    public ResetUserPasswordViewModel? ResetPasswordForm { get; set; }

    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
}
