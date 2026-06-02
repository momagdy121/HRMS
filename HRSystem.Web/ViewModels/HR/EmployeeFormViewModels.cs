using System.ComponentModel.DataAnnotations;
using HRSystem.Common.Constants;

namespace HRSystem.Web.ViewModels.HR;

public class CreateEmployeeViewModel
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(100)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required.")]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Salary must be zero or greater.")]
    [Display(Name = "Base Salary")]
    public decimal Salary { get; set; }

    [Required]
    [Display(Name = "Hire Date")]
    [DataType(DataType.Date)]
    public DateOnly HireDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    [Display(Name = "Role")]
    public string Role { get; set; } = RoleNames.Employee;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string InitialPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm the password.")]
    [DataType(DataType.Password)]
    [Compare(nameof(InitialPassword), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class EditEmployeeViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    [Display(Name = "Base Salary")]
    public decimal Salary { get; set; }

    [Required]
    [Display(Name = "Hire Date")]
    [DataType(DataType.Date)]
    public DateOnly HireDate { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}
