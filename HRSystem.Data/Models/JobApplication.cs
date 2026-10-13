using System.ComponentModel.DataAnnotations;
using HRSystem.Common.Enums;

namespace HRSystem.Data.Models;

public class JobApplication
{
    public int Id { get; set; }

    public int JobPostingId { get; set; }

    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(255)]
    public string CvPublicId { get; set; } = string.Empty;

    [MaxLength(255)]
    public string CvOriginalFileName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string CvStorageUrl { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CvContentType { get; set; } = string.Empty;

    public long CvFileSize { get; set; }

    [MaxLength(4000)]
    public string? CoverLetter { get; set; }

    public JobApplicationStatus Status { get; set; } = JobApplicationStatus.Submitted;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(2000)]
    public string? ReviewNotes { get; set; }

    public int? ReviewedById { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public int? CreatedEmployeeId { get; set; }

    public bool RejectionEmailSent { get; set; }

    public DateTime? RejectionEmailSentAt { get; set; }

    public bool AcceptanceEmailSent { get; set; }

    public DateTime? AcceptanceEmailSentAt { get; set; }

    // ── Navigation Properties ──────────────────────────────────────
    public JobPosting JobPosting { get; set; } = null!;

    public Employee? ReviewedBy { get; set; }

    public Employee? CreatedEmployee { get; set; }
}
