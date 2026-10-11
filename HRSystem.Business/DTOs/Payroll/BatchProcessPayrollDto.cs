namespace HRSystem.Business.DTOs.Payroll;

public class BatchProcessPayrollDto
{
    public int Month { get; set; }

    public int Year { get; set; }

    /// <summary>When set, only employees in this department are included.</summary>
    public int? DepartmentId { get; set; }
}
