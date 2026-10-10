using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace HRSystem.Data.Models;

public class Employee : IdentityUser<int>
{
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public bool IsHR { get; set; }

    public int DepartmentId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Salary { get; set; }

    public DateOnly HireDate { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsPasswordChangeRequired { get; set; } = true;

    // ── Navigation Properties ──────────────────────────────────────
    public Department Department { get; set; } = null!;

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
