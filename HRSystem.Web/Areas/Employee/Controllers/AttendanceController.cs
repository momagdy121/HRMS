using HRSystem.Business.Exceptions;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Data.Interfaces;
using HRSystem.Web.Helpers;
using HRSystem.Web.ViewModels.Attendance;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.Employee.Controllers;

public class AttendanceController : EmployeeBaseController
{
    private const int PageSize = 10;
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IAttendanceService attendanceService)
        : base(currentUser, unitOfWork)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, bool showCheckIn = false, bool showCheckOut = false)
    {
        await SetLayoutAsync("Attendance", "Search attendance...");
        return View(await BuildIndexModelAsync(page, showCheckIn, showCheckOut));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn()
    {
        try
        {
            await _attendanceService.CheckInAsync();
            TempData["Success"] = "Checked in successfully.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut()
    {
        try
        {
            await _attendanceService.CheckOutAsync();
            TempData["Success"] = "Checked out successfully.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<EmployeeAttendanceIndexViewModel> BuildIndexModelAsync(
        int page,
        bool showCheckIn,
        bool showCheckOut)
    {
        var employee = await CurrentUser.GetCurrentEmployeeAsync();
        var paged = await _attendanceService.GetByEmployeeAsync(employee.Id, page, PageSize);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayAttendance = await UnitOfWork.Attendances.GetByEmployeeAndDateAsync(employee.Id, today);

        var canCheckIn = todayAttendance?.CheckInTime == null;
        var canCheckOut = todayAttendance?.CheckInTime != null && todayAttendance.CheckOutTime == null;

        var records = paged.Items.Select(a =>
        {
            var label = AttendanceDisplayHelper.GetStatusLabel(a);
            return new AttendanceListItemViewModel
            {
                Id = a.Id,
                Date = a.Date,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                StatusLabel = label,
                DurationLabel = AttendanceDisplayHelper.FormatDuration(a.CheckInTime, a.CheckOutTime)
            };
        }).ToList();

        return new EmployeeAttendanceIndexViewModel
        {
            CanCheckIn = canCheckIn,
            CanCheckOut = canCheckOut,
            ShowCheckInModal = showCheckIn && canCheckIn,
            ShowCheckOutModal = showCheckOut && canCheckOut,
            Records = records,
            Page = paged.Page,
            TotalPages = paged.TotalPages,
            TotalCount = paged.TotalCount,
            PageSize = paged.PageSize
        };
    }
}
