using HRSystem.Common.Enums;
using HRSystem.Data.Context;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.Data.Repositories;

public class PayrollItemRepository : IPayrollItemRepository
{
    private readonly AppDbContext _context;

    public PayrollItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PayrollItem>> GetByPayrollIdAsync(int payrollId, CancellationToken cancellationToken = default) =>
        await _context.PayrollItems
            .AsNoTracking()
            .Where(i => i.PayrollId == payrollId)
            .OrderBy(i => i.Id)
            .ToListAsync(cancellationToken);

    public Task<decimal> GetBonusTotalAsync(int payrollId, CancellationToken cancellationToken = default) =>
        _context.PayrollItems
            .Where(i => i.PayrollId == payrollId && i.ItemType == ItemType.Bonus)
            .SumAsync(i => i.Amount, cancellationToken);

    public Task<decimal> GetDeductionTotalAsync(int payrollId, CancellationToken cancellationToken = default) =>
        _context.PayrollItems
            .Where(i => i.PayrollId == payrollId && i.ItemType == ItemType.Deduction)
            .SumAsync(i => i.Amount, cancellationToken);

    public async Task AddAsync(PayrollItem item, CancellationToken cancellationToken = default) =>
        await _context.PayrollItems.AddAsync(item, cancellationToken);

    public Task<PayrollItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.PayrollItems.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public void Update(PayrollItem item) => _context.PayrollItems.Update(item);

    public void Delete(PayrollItem item) => _context.PayrollItems.Remove(item);
}
