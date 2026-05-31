using Microsoft.EntityFrameworkCore.Storage;

namespace HRSystem.Data.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IEmployeeRepository Employees { get; }
    IDepartmentRepository Departments { get; }
    IEmployeeTaskRepository EmployeeTasks { get; }
    IPayrollRepository Payrolls { get; }
    IPayrollItemRepository PayrollItems { get; }
    ILeaveRequestRepository LeaveRequests { get; }
    ILeaveBalanceRepository LeaveBalances { get; }
    IAttendanceRepository Attendances { get; }
    IApplicationUserRepository ApplicationUsers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
