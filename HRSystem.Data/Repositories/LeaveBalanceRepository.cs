using HRSystem.Common.Enums;
using HRSystem.Data.Context;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.Data.Repositories;

public class LeaveBalanceRepository : ILeaveBalanceRepository
{
    private readonly AppDbContext _context;

    public LeaveBalanceRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<LeaveBalance?> GetAsync(int employeeId, int year, LeaveType leaveType, CancellationToken cancellationToken = default) =>
        _context.LeaveBalances.FirstOrDefaultAsync(
            b => b.EmployeeId == employeeId && b.Year == year && b.LeaveType == leaveType,
            cancellationToken);

    public async Task AddAsync(LeaveBalance leaveBalance, CancellationToken cancellationToken = default) =>
        await _context.LeaveBalances.AddAsync(leaveBalance, cancellationToken);

    public void Update(LeaveBalance leaveBalance) => _context.LeaveBalances.Update(leaveBalance);
}
