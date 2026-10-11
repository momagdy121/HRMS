namespace HRSystem.Business.DTOs.Payroll;

public class BatchProcessPayrollResult
{
    public int CreatedCount { get; set; }

    public int SkippedCount { get; set; }

    public IReadOnlyList<string> SkippedDetails { get; set; } = [];
}
