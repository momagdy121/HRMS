using HRSystem.Business.Exceptions;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Common.Enums;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using EmployeeEntity = HRSystem.Data.Models.Employee;
using HRSystem.Web.Helpers;
using HRSystem.Web.ViewModels.Leave;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.HR.Controllers;

public class LeaveController : HRBaseController
{
    private const int PageSize = 10;
    private readonly ILeaveService _leaveService;

    public LeaveController(
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        ILeaveService leaveService)
        : base(currentUser, unitOfWork)
    {
        _leaveService = leaveService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int page = 1,
        LeaveRequestStatus? status = null,
        int? approveId = null,
        int? rejectId = null)
    {
        await SetLayoutAsync("Leave", "Search leave requests...");
        return View(await BuildIndexModelAsync(page, status, approveId, rejectId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            await _leaveService.ApproveAsync(id);
            TempData["Success"] = "Leave request approved.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject([Bind(Prefix = "RejectForm")] RejectLeaveViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Leave", "Search leave requests...");
            return View("Index", await BuildIndexModelAsync(1, rejectId: model.Id, rejectForm: model));
        }

        try
        {
            await _leaveService.RejectAsync(model.Id, model.RejectionReason);
            TempData["Success"] = "Leave request rejected.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("Leave", "Search leave requests...");
            ModalValidationHelper.AddFormErrors(ModelState, "RejectForm", ex.Message);
            return View("Index", await BuildIndexModelAsync(1, rejectId: model.Id, rejectForm: model));
        }
    }

    private async Task<HrLeaveIndexViewModel> BuildIndexModelAsync(
        int page,
        LeaveRequestStatus? status = null,
        int? approveId = null,
        int? rejectId = null,
        RejectLeaveViewModel? rejectForm = null)
    {
        var paged = await _leaveService.GetFilteredAsync(status, page, PageSize);
        var employees = await LoadEmployeesAsync(paged.Items);

        var requests = paged.Items
            .Select(r => MapListItem(r, employees, canAction: r.Status == LeaveRequestStatus.Pending))
            .ToList();

        LeaveActionTargetViewModel? approveTarget = null;
        if (approveId.HasValue)
        {
            approveTarget = await BuildActionTargetAsync(approveId.Value, paged.Items, employees);
        }

        RejectLeaveViewModel? resolvedRejectForm = null;
        if (rejectId.HasValue || rejectForm != null)
        {
            var id = rejectForm?.Id ?? rejectId!.Value;
            var target = await BuildActionTargetAsync(id, paged.Items, employees);
            if (target != null)
            {
                resolvedRejectForm = rejectForm ?? new RejectLeaveViewModel
                {
                    Id = target.Id,
                    EmployeeName = target.EmployeeName,
                    LeaveType = target.LeaveType,
                    StartDate = target.StartDate,
                    EndDate = target.EndDate
                };
            }
        }

        return new HrLeaveIndexViewModel
        {
            Requests = requests,
            StatusFilter = status,
            ApproveTarget = approveTarget,
            RejectForm = resolvedRejectForm,
            Page = paged.Page,
            TotalPages = paged.TotalPages,
            TotalCount = paged.TotalCount,
            PageSize = paged.PageSize
        };
    }

    private async Task<LeaveActionTargetViewModel?> BuildActionTargetAsync(
        int id,
        IReadOnlyList<LeaveRequest> pageRequests,
        IReadOnlyDictionary<int, EmployeeEntity> employees)
    {
        var request = pageRequests.FirstOrDefault(r => r.Id == id)
                      ?? await UnitOfWork.LeaveRequests.GetByIdAsync(id);

        if (request == null || request.Status != LeaveRequestStatus.Pending)
            return null;

        employees.TryGetValue(request.EmployeeId, out var employee);

        return new LeaveActionTargetViewModel
        {
            Id = request.Id,
            EmployeeName = employee != null ? TaskDisplayHelper.GetFullName(employee) : "Unknown",
            LeaveType = request.LeaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Days = LeaveDisplayHelper.GetDayCount(request.StartDate, request.EndDate)
        };
    }

    private static LeaveListItemViewModel MapListItem(
        LeaveRequest request,
        IReadOnlyDictionary<int, EmployeeEntity> employees,
        bool canAction)
    {
        employees.TryGetValue(request.EmployeeId, out var employee);

        return new LeaveListItemViewModel
        {
            Id = request.Id,
            EmployeeId = request.EmployeeId,
            EmployeeName = employee != null ? TaskDisplayHelper.GetFullName(employee) : "Unknown",
            EmployeeInitials = employee != null ? TaskDisplayHelper.GetInitials(employee) : "?",
            LeaveType = request.LeaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Days = LeaveDisplayHelper.GetDayCount(request.StartDate, request.EndDate),
            Reason = request.Reason,
            Status = request.Status,
            CanAction = canAction
        };
    }

    private async Task<Dictionary<int, EmployeeEntity>> LoadEmployeesAsync(IReadOnlyList<LeaveRequest> requests)
    {
        var employees = new Dictionary<int, EmployeeEntity>();
        foreach (var id in requests.Select(r => r.EmployeeId).Distinct())
        {
            var employee = await UnitOfWork.Employees.GetByIdAsync(id);
            if (employee != null)
                employees[id] = employee;
        }

        return employees;
    }
}
