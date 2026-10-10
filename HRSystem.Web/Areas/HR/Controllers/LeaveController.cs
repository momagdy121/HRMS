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
    private const int PageSize = PaginationDefaults.DefaultPageSize;
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
        string? search = null,
        int? approveId = null,
        int? rejectId = null)
    {
        await SetLayoutAsync("Leave", "Search leave requests...");
        return View(await BuildIndexModelAsync(page, status, search, approveId, rejectId));
    }

    [HttpGet]
    public async Task<IActionResult> Balances(
        int? departmentId = null,
        int? year = null,
        string? search = null,
        int page = 1,
        int? adjustEmployeeId = null,
        LeaveType? adjustLeaveType = null)
    {
        await SetLayoutAsync("Leave", "Search employee leave balances...");
        return View(await BuildBalancesModelAsync(departmentId, year, search, page, adjustEmployeeId, adjustLeaveType));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdjustBalance(
        [Bind(Prefix = "AdjustForm")] AdjustLeaveBalanceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Leave", "Search employee leave balances...");
            var invalid = await BuildBalancesModelAsync(null, model.Year, null, 1);
            invalid.AdjustForm = model;
            return View("Balances", invalid);
        }

        try
        {
            await _leaveService.AdjustBalanceAsync(
                model.EmployeeId, model.Year, model.LeaveType, model.TotalDays, model.UsedDays);
            TempData["Success"] = $"Leave balance for {model.EmployeeName} updated successfully.";
            return RedirectToAction(nameof(Balances), new { year = model.Year });
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Balances), new { year = model.Year });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rollover(int fromYear, int toYear)
    {
        try
        {
            await _leaveService.RolloverBalancesAsync(fromYear, toYear);
            TempData["Success"] = $"Annual leave balances rolled over from {fromYear} to {toYear}.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Balances), new { year = toYear });
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
        string? search = null,
        int? approveId = null,
        int? rejectId = null,
        RejectLeaveViewModel? rejectForm = null)
    {
        var paged = await _leaveService.GetFilteredAsync(status, page, PageSize, search);
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

    private Task<Dictionary<int, EmployeeEntity>> LoadEmployeesAsync(IReadOnlyList<LeaveRequest> requests) =>
        UnitOfWork.Employees.GetByIdsAsync(requests.Select(r => r.EmployeeId));

    private async Task<HrLeaveBalancesViewModel> BuildBalancesModelAsync(
        int? departmentId,
        int? year,
        string? search,
        int page,
        int? adjustEmployeeId = null,
        LeaveType? adjustLeaveType = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var departments = await HrDisplayHelper.LoadDepartmentsAsync(UnitOfWork);

        var pagedEmployees = departmentId.HasValue
            ? await UnitOfWork.Employees.GetByDepartmentPagedAsync(departmentId.Value, page, PageSize, search)
            : await UnitOfWork.Employees.GetActivePagedAsync(page, PageSize, search);

        var items = new List<HrEmployeeLeaveBalanceItemViewModel>();
        foreach (var emp in pagedEmployees.Items)
        {
            departments.TryGetValue(emp.DepartmentId, out var dept);
            var balances = await _leaveService.GetEmployeeBalancesAsync(emp.Id, targetYear);
            var annual = balances.FirstOrDefault(b => b.LeaveType == LeaveType.Annual);
            var sick = balances.FirstOrDefault(b => b.LeaveType == LeaveType.Sick);

            items.Add(new HrEmployeeLeaveBalanceItemViewModel
            {
                EmployeeId = emp.Id,
                EmployeeName = $"{emp.FirstName} {emp.LastName}",
                EmployeeInitials = HrDisplayHelper.GetInitials(emp.FirstName, emp.LastName),
                DepartmentName = dept?.Name ?? "—",
                AnnualTotal = annual?.TotalDays ?? 21,
                AnnualUsed = annual?.UsedDays ?? 0,
                SickTotal = sick?.TotalDays ?? 10,
                SickUsed = sick?.UsedDays ?? 0
            });
        }

        AdjustLeaveBalanceViewModel? adjustForm = null;
        if (adjustEmployeeId.HasValue)
        {
            var emp = await UnitOfWork.Employees.GetByIdAsync(adjustEmployeeId.Value);
            if (emp != null)
            {
                var targetType = adjustLeaveType ?? LeaveType.Annual;
                var balance = await _leaveService.GetBalanceAsync(emp.Id, targetYear, targetType);
                adjustForm = new AdjustLeaveBalanceViewModel
                {
                    EmployeeId = emp.Id,
                    EmployeeName = $"{emp.FirstName} {emp.LastName}",
                    Year = targetYear,
                    LeaveType = targetType,
                    TotalDays = balance?.TotalDays ?? HRSystem.Business.Helpers.LeaveBalanceDefaults.TotalDaysFor(targetType),
                    UsedDays = balance?.UsedDays ?? 0
                };
            }
        }

        return new HrLeaveBalancesViewModel
        {
            Balances = items,
            Departments = departments.Values.Select(d => new HRSystem.Web.ViewModels.HR.DepartmentOptionViewModel { Id = d.Id, Name = d.Name }).OrderBy(d => d.Name).ToList(),
            SelectedYear = targetYear,
            DepartmentFilter = departmentId,
            AdjustForm = adjustForm,
            Page = pagedEmployees.Page,
            TotalPages = pagedEmployees.TotalPages,
            TotalCount = pagedEmployees.TotalCount,
            PageSize = pagedEmployees.PageSize
        };
    }
}
