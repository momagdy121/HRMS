using System.ComponentModel.DataAnnotations;
using HRSystem.Common.Enums;
using HRSystem.Data.Common;
using HRSystem.Data.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRSystem.Web.ViewModels.Recruitment;

public class JobPostingListViewModel
{
    public PagedList<JobPosting> Postings { get; set; } = new();
    public JobPostingStatus? Status { get; set; }
    public int? DepartmentId { get; set; }
    public string? Search { get; set; }
    public List<SelectListItem> Departments { get; set; } = [];
}

public class JobPostingFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Job title is required.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Job title must be between 3 and 200 characters.")]
    [Display(Name = "Job Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid department.")]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "Job description is required.")]
    [Display(Name = "Job Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Requirements are required.")]
    [Display(Name = "Requirements")]
    public string Requirements { get; set; } = string.Empty;

    [Required(ErrorMessage = "Closing date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Application Closing Date")]
    public DateOnly ClosingDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14));

    public List<SelectListItem> Departments { get; set; } = [];

    public bool IsEdit => Id > 0;
}

public class ApplicationListViewModel
{
    public PagedList<JobApplication> Applications { get; set; } = new();
    public int? JobPostingId { get; set; }
    public JobApplicationStatus? Status { get; set; }
    public string? Search { get; set; }
    public List<SelectListItem> JobPostings { get; set; } = [];
}

public class ApplicationDetailsViewModel
{
    public JobApplication Application { get; set; } = null!;
}

public class OnboardCandidateViewModel
{
    public int ApplicationId { get; set; }

    public string CandidateFullName { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 100 characters.")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 100 characters.")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    [StringLength(256)]
    [Display(Name = "Work Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a department.")]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "Salary is required.")]
    [Range(0, 10000000, ErrorMessage = "Salary must be a positive number.")]
    [Display(Name = "Salary")]
    public decimal Salary { get; set; }

    [Required(ErrorMessage = "Hire date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Hire Date")]
    public DateOnly HireDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [Required(ErrorMessage = "Role is required.")]
    [Display(Name = "Assigned Role")]
    public string Role { get; set; } = string.Empty;

    [Required(ErrorMessage = "Initial password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "Temporary Initial Password")]
    public string InitialPassword { get; set; } = string.Empty;

    public List<SelectListItem> Departments { get; set; } = [];

    public List<SelectListItem> AllowedRoles { get; set; } = [];
}
