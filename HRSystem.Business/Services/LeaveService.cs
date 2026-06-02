using HRSystem.Business.DTOs;
using HRSystem.Business.DTOs.Leave;
using HRSystem.Business.Exceptions;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Business.Helpers;
using HRSystem.Business.Mapping;
using HRSystem.Common.Enums;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;

namespace HRSystem.Business.Services;

public class LeaveService : ILeaveService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public LeaveService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<LeaveRequest> RequestAsync(RequestLeaveDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.EndDate < dto.StartDate)
            throw new BusinessRuleException("End date must be on or after start date.");

        var employee = await _currentUser.GetCurrentEmployeeAsync(cancellationToken);

        if (await _unitOfWork.LeaveRequests.HasOverlappingApprovedAsync(
                employee.Id, dto.StartDate, dto.EndDate, cancellationToken: cancellationToken))
        {
            throw new BusinessRuleException("Leave dates overlap an existing approved leave.");
        }

        if (dto.LeaveType is LeaveType.Annual or LeaveType.Sick)
        {
            var year = dto.StartDate.Year;
            var balance = await _unitOfWork.LeaveBalances.GetAsync(employee.Id, year, dto.LeaveType, cancellationToken)
                          ?? throw new BusinessRuleException($"No {dto.LeaveType} leave balance found for {year}.");

            var requestedDays = LeaveHelper.CalendarDays(dto.StartDate, dto.EndDate);
            var remaining = balance.TotalDays - balance.UsedDays;
            if (requestedDays > remaining)
            {
                throw new BusinessRuleException(
                    $"Insufficient {dto.LeaveType} leave balance. Remaining: {remaining} day(s).");
            }
        }

        var request = LeaveRequestMapper.FromDto(dto, employee.Id);

        await _unitOfWork.LeaveRequests.AddAsync(request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task ApproveAsync(int leaveRequestId, CancellationToken cancellationToken = default)
    {
        var request = await GetLeaveRequestAsync(leaveRequestId, cancellationToken);
        if (request.Status != LeaveRequestStatus.Pending)
            throw new BusinessRuleException("Only pending leave requests can be approved.");

        await EnsureCanActionLeaveAsync(request, cancellationToken);

        var approverId = (await _currentUser.GetCurrentEmployeeAsync(cancellationToken)).Id;
        LeaveWorkflow.Approve(request, approverId);

        await ApplyBalanceChangeAsync(request, +LeaveHelper.CalendarDays(request.StartDate, request.EndDate), cancellationToken);

        _unitOfWork.LeaveRequests.Update(request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectAsync(int leaveRequestId, string rejectionReason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rejectionReason))
            throw new BusinessRuleException("Rejection reason is required.");

        var request = await GetLeaveRequestAsync(leaveRequestId, cancellationToken);
        if (request.Status is not (LeaveRequestStatus.Pending or LeaveRequestStatus.Approved))
            throw new BusinessRuleException("Only pending or approved leave requests can be rejected.");

        await EnsureCanActionLeaveAsync(request, cancellationToken);

        var wasApproved = request.Status == LeaveRequestStatus.Approved;
        var days = LeaveHelper.CalendarDays(request.StartDate, request.EndDate);

        LeaveWorkflow.Reject(request, rejectionReason);

        if (wasApproved)
            await ApplyBalanceChangeAsync(request, -days, cancellationToken);

        _unitOfWork.LeaveRequests.Update(request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<LeaveBalance?> GetBalanceAsync(int employeeId, int year, LeaveType leaveType, CancellationToken cancellationToken = default) =>
        await _unitOfWork.LeaveBalances.GetAsync(employeeId, year, leaveType, cancellationToken);

    public async Task<PagedResult<LeaveRequest>> GetPendingByDepartmentAsync(int departmentId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.LeaveRequests.GetPendingByDepartmentPagedAsync(departmentId, page, pageSize, cancellationToken);
        return PagedResultMapper.Map(result);
    }

    public async Task<PagedResult<LeaveRequest>> GetAllPendingAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.LeaveRequests.GetAllPendingPagedAsync(page, pageSize, cancellationToken);
        return PagedResultMapper.Map(result);
    }

    public async Task<PagedResult<LeaveRequest>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.LeaveRequests.GetAllPagedAsync(page, pageSize, cancellationToken);
        return PagedResultMapper.Map(result);
    }

    public async Task<PagedResult<LeaveRequest>> GetFilteredAsync(
        LeaveRequestStatus? status,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.LeaveRequests.GetFilteredPagedAsync(status, page, pageSize, cancellationToken);
        return PagedResultMapper.Map(result);
    }

    public async Task<PagedResult<LeaveRequest>> GetByEmployeeAsync(int employeeId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.LeaveRequests.GetByEmployeePagedAsync(employeeId, page, pageSize, cancellationToken);
        return PagedResultMapper.Map(result);
    }

    public async Task<PagedResult<LeaveRequest>> GetByDepartmentAsync(
        int departmentId,
        LeaveRequestStatus? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.LeaveRequests.GetByDepartmentPagedAsync(
            departmentId, status, page, pageSize, cancellationToken);
        return PagedResultMapper.Map(result);
    }

    public async Task<IReadOnlyList<LeaveBalance>> GetEmployeeBalancesAsync(
        int employeeId,
        int year,
        CancellationToken cancellationToken = default) =>
        await _unitOfWork.LeaveBalances.GetByEmployeeAndYearAsync(employeeId, year, cancellationToken);

    private async Task<LeaveRequest> GetLeaveRequestAsync(int id, CancellationToken cancellationToken) =>
        await _unitOfWork.LeaveRequests.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException("Leave request not found.", "Leave", "Index", "HR");

    private async Task EnsureCanActionLeaveAsync(LeaveRequest request, CancellationToken cancellationToken)
    {
        var approver = await _currentUser.GetCurrentEmployeeAsync(cancellationToken);
        var requester = await _unitOfWork.Employees.GetByIdAsync(request.EmployeeId, cancellationToken)
                        ?? throw new NotFoundException("Employee not found.");

        if (_currentUser.IsDepartmentHead() && !_currentUser.IsHR())
        {
            if (approver.Id == request.EmployeeId)
            {
                throw new BusinessRuleException(
                    "Department heads cannot approve or reject their own leave requests.");
            }

            if (requester.DepartmentId != approver.DepartmentId)
            {
                throw new BusinessRuleException(
                    "You can only action leave requests for employees in your department.");
            }

            return;
        }

        if (!_currentUser.IsHR())
            throw new BusinessRuleException("You are not authorized to action leave requests.");
    }

    private async Task ApplyBalanceChangeAsync(LeaveRequest request, int dayDelta, CancellationToken cancellationToken)
    {
        if (request.LeaveType is LeaveType.Unpaid)
            return;

        var balance = await _unitOfWork.LeaveBalances.GetAsync(
                          request.EmployeeId, request.StartDate.Year, request.LeaveType, cancellationToken)
                      ?? throw new BusinessRuleException(
                          $"No {request.LeaveType} leave balance found for {request.StartDate.Year}.");

        balance.UsedDays += dayDelta;
        if (balance.UsedDays < 0)
            balance.UsedDays = 0;

        _unitOfWork.LeaveBalances.Update(balance);
    }
}
