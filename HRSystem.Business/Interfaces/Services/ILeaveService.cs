using HRSystem.Business.DTOs;
using HRSystem.Business.DTOs.Leave;
using HRSystem.Common.Enums;
using HRSystem.Data.Models;

namespace HRSystem.Business.Interfaces.Services;

public interface ILeaveService
{
    Task<LeaveRequest> RequestAsync(RequestLeaveDto dto, CancellationToken cancellationToken = default);

    Task ApproveAsync(int leaveRequestId, CancellationToken cancellationToken = default);

    Task RejectAsync(int leaveRequestId, string rejectionReason, CancellationToken cancellationToken = default);

    Task<LeaveBalance?> GetBalanceAsync(int employeeId, int year, LeaveType leaveType, CancellationToken cancellationToken = default);

    Task<PagedResult<LeaveRequest>> GetPendingByDepartmentAsync(int departmentId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    Task<PagedResult<LeaveRequest>> GetAllPendingAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    Task<PagedResult<LeaveRequest>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
}
