using HRSystem.Data.Context;
using HRSystem.Data.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace HRSystem.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(
        AppDbContext context,
        IEmployeeRepository employees,
        IDepartmentRepository departments,
        IEmployeeTaskRepository employeeTasks,
        IPayrollRepository payrolls,
        IPayrollItemRepository payrollItems,
        ILeaveRequestRepository leaveRequests,
        ILeaveBalanceRepository leaveBalances,
        IAttendanceRepository attendances,
        IApplicationUserRepository applicationUsers)
    {
        _context = context;
        Employees = employees;
        Departments = departments;
        EmployeeTasks = employeeTasks;
        Payrolls = payrolls;
        PayrollItems = payrollItems;
        LeaveRequests = leaveRequests;
        LeaveBalances = leaveBalances;
        Attendances = attendances;
        ApplicationUsers = applicationUsers;
    }

    public IEmployeeRepository Employees { get; }

    public IDepartmentRepository Departments { get; }

    public IEmployeeTaskRepository EmployeeTasks { get; }

    public IPayrollRepository Payrolls { get; }

    public IPayrollItemRepository PayrollItems { get; }

    public ILeaveRequestRepository LeaveRequests { get; }

    public ILeaveBalanceRepository LeaveBalances { get; }

    public IAttendanceRepository Attendances { get; }

    public IApplicationUserRepository ApplicationUsers { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        _context.Database.BeginTransactionAsync(cancellationToken);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
