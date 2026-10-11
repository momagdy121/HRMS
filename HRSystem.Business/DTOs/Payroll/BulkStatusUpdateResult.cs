namespace HRSystem.Business.DTOs.Payroll;

public class BulkStatusUpdateResult
{
    public int UpdatedCount { get; set; }

    public int SkippedCount { get; set; }

    public IReadOnlyList<string> SkippedDetails { get; set; } = [];
}
