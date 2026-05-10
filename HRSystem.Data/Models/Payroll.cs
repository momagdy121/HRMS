using System.ComponentModel.DataAnnotations.Schema;
using HRSystem.Common.Enums;

namespace HRSystem.Data.Models;

public class Payroll
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BaseSalary { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalBonus { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDeduction { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal NetSalary { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;

    public int ProcessedBy { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
