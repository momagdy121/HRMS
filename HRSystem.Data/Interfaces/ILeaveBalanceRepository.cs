using HRSystem.Common.Enums;
using HRSystem.Data.Models;

namespace HRSystem.Data.Interfaces;

public interface ILeaveBalanceRepository
{
    Task<LeaveBalance?> GetAsync(int employeeId, int year, LeaveType leaveType, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaveBalance>> GetByEmployeeAndYearAsync(int employeeId, int year, CancellationToken cancellationToken = default);

    Task AddAsync(LeaveBalance leaveBalance, CancellationToken cancellationToken = default);

    void Update(LeaveBalance leaveBalance);
}
