using HRSystem.Data.Models;

namespace HRSystem.Data.Interfaces;

public interface IPayrollItemRepository
{
    Task<IReadOnlyList<PayrollItem>> GetByPayrollIdAsync(int payrollId, CancellationToken cancellationToken = default);

    Task<decimal> GetBonusTotalAsync(int payrollId, CancellationToken cancellationToken = default);

    Task<decimal> GetDeductionTotalAsync(int payrollId, CancellationToken cancellationToken = default);

    Task AddAsync(PayrollItem item, CancellationToken cancellationToken = default);
}
