using HRSystem.Business.DTOs.Leave;
using HRSystem.Business.Exceptions;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Common.Enums;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using HRSystem.Web.Helpers;
using HRSystem.Web.ViewModels.Leave;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.Employee.Controllers;

public class LeaveController : EmployeeBaseController
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
    public async Task<IActionResult> Index(int page = 1, bool showRequest = false)
    {
        await SetLayoutAsync("Leave", "Search leave requests...");
        return View(await BuildIndexModelAsync(page, showRequest));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitRequest([Bind(Prefix = "RequestForm")] RequestLeaveViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Leave", "Search leave requests...");
            return View("Index", await BuildIndexModelAsync(1, showRequest: true, requestForm: model));
        }

        try
        {
            await _leaveService.RequestAsync(new RequestLeaveDto
            {
                LeaveType = model.LeaveType,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Reason = model.Reason
            });

            TempData["Success"] = "Leave request submitted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("Leave", "Search leave requests...");
            ModalValidationHelper.AddFormErrors(ModelState, "RequestForm", ex.Message);
            return View("Index", await BuildIndexModelAsync(1, showRequest: true, requestForm: model));
        }
    }

    private async Task<EmployeeLeaveIndexViewModel> BuildIndexModelAsync(
        int page,
        bool showRequest = false,
        RequestLeaveViewModel? requestForm = null)
    {
        var employee = await CurrentUser.GetCurrentEmployeeAsync();
        var year = DateTime.UtcNow.Year;
        var balances = await _leaveService.GetEmployeeBalancesAsync(employee.Id, year);
        var paged = await _leaveService.GetByEmployeeAsync(employee.Id, page, PageSize);

        var annual = balances.FirstOrDefault(b => b.LeaveType == LeaveType.Annual);
        var sick = balances.FirstOrDefault(b => b.LeaveType == LeaveType.Sick);

        var requests = paged.Items
            .Select(MapListItem)
            .ToList();

        return new EmployeeLeaveIndexViewModel
        {
            Balances = balances
                .Where(b => b.LeaveType is LeaveType.Annual or LeaveType.Sick)
                .Select(b => new LeaveBalanceCardViewModel
                {
                    LeaveType = b.LeaveType,
                    TotalDays = b.TotalDays,
                    UsedDays = b.UsedDays
                })
                .ToList(),
            AnnualRemaining = annual != null ? annual.TotalDays - annual.UsedDays : 0,
            SickRemaining = sick != null ? sick.TotalDays - sick.UsedDays : 0,
            UsedThisYear = balances
                .Where(b => b.LeaveType is LeaveType.Annual or LeaveType.Sick)
                .Sum(b => b.UsedDays),
            Requests = requests,
            RequestForm = requestForm ?? new RequestLeaveViewModel(),
            ShowRequestModal = showRequest,
            Page = paged.Page,
            TotalPages = paged.TotalPages,
            TotalCount = paged.TotalCount,
            PageSize = paged.PageSize
        };
    }

    private static LeaveListItemViewModel MapListItem(LeaveRequest request) => new()
    {
        Id = request.Id,
        EmployeeId = request.EmployeeId,
        LeaveType = request.LeaveType,
        StartDate = request.StartDate,
        EndDate = request.EndDate,
        Days = LeaveDisplayHelper.GetDayCount(request.StartDate, request.EndDate),
        Reason = request.Reason,
        Status = request.Status
    };
}
