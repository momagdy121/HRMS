using HRSystem.Data.Common;
using HRSystem.Data.Models;

namespace HRSystem.Data.Interfaces;

public interface ILeaveRequestRepository
{
    Task<LeaveRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedList<LeaveRequest>> GetPendingByDepartmentPagedAsync(int departmentId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<PagedList<LeaveRequest>> GetAllPendingPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<PagedList<LeaveRequest>> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<bool> HasOverlappingApprovedAsync(int employeeId, DateOnly startDate, DateOnly endDate, int? excludeRequestId = null, CancellationToken cancellationToken = default);

    Task AddAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);

    void Update(LeaveRequest leaveRequest);
}
