using HRSystem.Common.Enums;

namespace HRSystem.Business.DTOs.Payroll;

public class EditPayrollItemDto
{
    public int Id { get; set; }
    public int PayrollId { get; set; }
    public ItemType ItemType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
