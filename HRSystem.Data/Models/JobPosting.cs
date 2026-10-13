using System.ComponentModel.DataAnnotations;
using HRSystem.Common.Enums;

namespace HRSystem.Data.Models;

public class JobPosting
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Requirements { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public DateOnly ClosingDate { get; set; }

    public JobPostingStatus Status { get; set; } = JobPostingStatus.Draft;

    public int CreatedById { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PublishedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    // ── Navigation Properties ──────────────────────────────────────
    public Department Department { get; set; } = null!;

    public Employee CreatedBy { get; set; } = null!;

    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
