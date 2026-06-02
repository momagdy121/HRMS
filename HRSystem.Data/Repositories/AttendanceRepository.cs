using HRSystem.Data.Common;
using HRSystem.Data.Context;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.Data.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly AppDbContext _context;

    public AttendanceRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Attendance?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Attendances.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<Attendance?> GetByEmployeeAndDateAsync(int employeeId, DateOnly date, CancellationToken cancellationToken = default) =>
        _context.Attendances.FirstOrDefaultAsync(
            a => a.EmployeeId == employeeId && a.Date == date && !a.IsDeleted,
            cancellationToken);

    public Task<PagedList<Attendance>> GetByEmployeePagedAsync(int employeeId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        _context.Attendances
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.EmployeeId == employeeId)
            .OrderByDescending(a => a.Date)
            .ToPagedListAsync(page, pageSize, cancellationToken);

    public Task<PagedList<Attendance>> GetByDepartmentPagedAsync(int departmentId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        _context.Attendances
            .AsNoTracking()
            .Where(a => !a.IsDeleted)
            .Where(a => _context.Employees.Any(e => e.Id == a.EmployeeId && e.DepartmentId == departmentId && !e.IsDeleted))
            .OrderByDescending(a => a.Date)
            .ToPagedListAsync(page, pageSize, cancellationToken);

    public Task<PagedList<Attendance>> GetReportPagedAsync(
        DateOnly date,
        int? departmentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query =
            from attendance in _context.Attendances.AsNoTracking()
            join employee in _context.Employees.AsNoTracking() on attendance.EmployeeId equals employee.Id
            where !attendance.IsDeleted
                  && !employee.IsDeleted
                  && attendance.Date == date
            select attendance;

        if (departmentId.HasValue)
        {
            query =
                from attendance in query
                join employee in _context.Employees.AsNoTracking() on attendance.EmployeeId equals employee.Id
                where employee.DepartmentId == departmentId.Value
                select attendance;
        }

        return query
            .OrderBy(a => a.EmployeeId)
            .ToPagedListAsync(page, pageSize, cancellationToken);
    }

    public Task<int> SoftDeleteAllForEmployeeAsync(int employeeId, CancellationToken cancellationToken = default) =>
        _context.Attendances
            .Where(a => !a.IsDeleted && a.EmployeeId == employeeId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(a => a.IsDeleted, true),
                cancellationToken);

    public async Task AddAsync(Attendance attendance, CancellationToken cancellationToken = default) =>
        await _context.Attendances.AddAsync(attendance, cancellationToken);

    public void Update(Attendance attendance) => _context.Attendances.Update(attendance);
}
