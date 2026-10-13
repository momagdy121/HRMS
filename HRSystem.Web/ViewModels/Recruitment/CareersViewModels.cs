using System.ComponentModel.DataAnnotations;
using HRSystem.Data.Common;
using HRSystem.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRSystem.Web.ViewModels.Recruitment;

public class CareersIndexViewModel
{
    public PagedList<JobPosting> Postings { get; set; } = new();
    public int? DepartmentId { get; set; }
    public string? Search { get; set; }
    public List<SelectListItem> Departments { get; set; } = [];
}

public class JobDetailsViewModel
{
    public JobPosting Posting { get; set; } = null!;
    public bool IsAcceptingApplications { get; set; }
    public int DaysRemaining { get; set; }
}

public class ApplyJobViewModel
{
    public int JobPostingId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateOnly ClosingDate { get; set; }

    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Full Name must be between 2 and 150 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    [StringLength(256)]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Please provide a valid phone number.")]
    [StringLength(30)]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Cover Letter (Optional)")]
    [StringLength(4000, ErrorMessage = "Cover letter cannot exceed 4000 characters.")]
    public string? CoverLetter { get; set; }

    [Required(ErrorMessage = "Please attach your CV document.")]
    [Display(Name = "CV / Resume (PDF, DOC, DOCX up to 5MB)")]
    public IFormFile CvFile { get; set; } = null!;
}

public class ApplySuccessViewModel
{
    public string CandidateName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}
