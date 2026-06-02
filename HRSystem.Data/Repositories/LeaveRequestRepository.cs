using HRSystem.Common.Enums;
using HRSystem.Data.Common;
using HRSystem.Data.Context;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.Data.Repositories;

public class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly AppDbContext _context;

    public LeaveRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<LeaveRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.LeaveRequests.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public Task<PagedList<LeaveRequest>> GetPendingByDepartmentPagedAsync(int departmentId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        _context.LeaveRequests
            .AsNoTracking()
            .Where(l => l.Status == LeaveRequestStatus.Pending)
            .Where(l => _context.Employees.Any(e =>
                e.Id == l.EmployeeId && e.DepartmentId == departmentId && !e.IsDeleted))
            .OrderBy(l => l.RequestDate)
            .ToPagedListAsync(page, pageSize, cancellationToken);

    public Task<PagedList<LeaveRequest>> GetAllPendingPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default) =>
        _context.LeaveRequests
            .AsNoTracking()
            .Where(l => l.Status == LeaveRequestStatus.Pending)
            .OrderBy(l => l.RequestDate)
            .ToPagedListAsync(page, pageSize, cancellationToken);

    public Task<PagedList<LeaveRequest>> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default) =>
        _context.LeaveRequests
            .AsNoTracking()
            .OrderByDescending(l => l.RequestDate)
            .ToPagedListAsync(page, pageSize, cancellationToken);

    public Task<PagedList<LeaveRequest>> GetFilteredPagedAsync(
        LeaveRequestStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.LeaveRequests.AsNoTracking().AsQueryable();

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        return query
            .OrderByDescending(l => l.RequestDate)
            .ToPagedListAsync(page, pageSize, cancellationToken);
    }

    public Task<PagedList<LeaveRequest>> GetByEmployeePagedAsync(
        int employeeId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        _context.LeaveRequests
            .AsNoTracking()
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.RequestDate)
            .ToPagedListAsync(page, pageSize, cancellationToken);

    public Task<PagedList<LeaveRequest>> GetByDepartmentPagedAsync(
        int departmentId,
        LeaveRequestStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.LeaveRequests
            .AsNoTracking()
            .Where(l => _context.Employees.Any(e =>
                e.Id == l.EmployeeId && e.DepartmentId == departmentId && !e.IsDeleted));

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        return query
            .OrderByDescending(l => l.RequestDate)
            .ToPagedListAsync(page, pageSize, cancellationToken);
    }

    public Task<bool> HasOverlappingApprovedAsync(int employeeId, DateOnly startDate, DateOnly endDate, int? excludeRequestId = null, CancellationToken cancellationToken = default) =>
        _context.LeaveRequests.AnyAsync(
            l => l.EmployeeId == employeeId
                 && l.Status == LeaveRequestStatus.Approved
                 && (!excludeRequestId.HasValue || l.Id != excludeRequestId.Value)
                 && l.StartDate <= endDate
                 && l.EndDate >= startDate,
            cancellationToken);

    public async Task AddAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default) =>
        await _context.LeaveRequests.AddAsync(leaveRequest, cancellationToken);

    public void Update(LeaveRequest leaveRequest) => _context.LeaveRequests.Update(leaveRequest);
}
