using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRSystem.Data.Models;

public class Employee
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    public bool IsHR { get; set; }

    public int DepartmentId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Salary { get; set; }

    public DateOnly HireDate { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }
}
