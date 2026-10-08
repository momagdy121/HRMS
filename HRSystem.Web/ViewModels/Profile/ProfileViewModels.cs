using System.ComponentModel.DataAnnotations;

namespace HRSystem.Web.ViewModels.Profile;

public class UpdateProfileContactViewModel
{
    [Phone]
    [Display(Name = "Phone Number")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
    public string? PhoneNumber { get; set; }
}

public class EmployeeProfileViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string DepartmentManagerName { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateOnly HireDate { get; set; }
    public string Tenure { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public int AnnualLeaveTotal { get; set; }
    public int AnnualLeaveUsed { get; set; }
    public int AnnualLeaveRemaining => Math.Max(0, AnnualLeaveTotal - AnnualLeaveUsed);

    public int SickLeaveTotal { get; set; }
    public int SickLeaveUsed { get; set; }
    public int SickLeaveRemaining => Math.Max(0, SickLeaveTotal - SickLeaveUsed);

    public UpdateProfileContactViewModel UpdateContactForm { get; set; } = new();
}
