using HRSystem.Business.DTOs.Payroll;
using HRSystem.Common.Enums;
using HRSystem.Data.Models;

namespace HRSystem.Business.Mapping;

public static class PayrollMapper
{
    public static Payroll FromDto(ProcessPayrollDto dto, int processedByEmployeeId, decimal baseSalary) =>
        new()
        {
            EmployeeId = dto.EmployeeId,
            BaseSalary = baseSalary,
            TotalBonus = 0,
            TotalDeduction = 0,
            NetSalary = baseSalary,
            Month = dto.Month,
            Year = dto.Year,
            Status = PayrollStatus.Draft,
            ProcessedBy = processedByEmployeeId,
            ProcessedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
}

public static class PayrollItemMapper
{
    public static PayrollItem FromDto(AddPayrollItemDto dto) =>
        new()
        {
            PayrollId = dto.PayrollId,
            ItemType = dto.ItemType,
            Description = dto.Description.Trim(),
            Amount = dto.Amount
        };

    public static void UpdateFromDto(PayrollItem item, EditPayrollItemDto dto)
    {
        item.ItemType = dto.ItemType;
        item.Description = dto.Description.Trim();
        item.Amount = dto.Amount;
    }
}
