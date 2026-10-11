using HRSystem.Business.Exceptions;
using HRSystem.Business.Helpers;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Data.Interfaces;
using HRSystem.Web.Helpers;
using HRSystem.Web.ViewModels.Attendance;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.Employee.Controllers;

public class AttendanceController : EmployeeBaseController
{
    private const int PageSize = PaginationDefaults.DefaultPageSize;
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

    [HttpGet]
    public async Task<IActionResult> Timesheet(int? year = null, int? month = null)
    {
        await SetLayoutAsync("Attendance", "Search attendance...");
        var employee = await CurrentUser.GetCurrentEmployeeAsync();
        var y = year ?? DateTime.UtcNow.Year;
        var m = month ?? DateTime.UtcNow.Month;

        var timesheetDto = await _attendanceService.GetMonthlyTimesheetAsync(employee.Id, y, m);
        var departments = await HrDisplayHelper.LoadDepartmentsAsync(UnitOfWork);
        departments.TryGetValue(employee.DepartmentId, out var dept);

        var model = new EmployeeTimesheetViewModel
        {
            EmployeeId = employee.Id,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            DepartmentName = dept?.Name ?? "—",
            JobTitle = employee.IsHR ? "CEO" : "Staff Member",
            Year = y,
            Month = m,
            DaysPresent = timesheetDto.DaysPresent,
            WeekdayAbsences = timesheetDto.WeekdayAbsences,
            LateCheckIns = timesheetDto.LateCheckIns,
            TotalHours = timesheetDto.TotalHours,
            Days = timesheetDto.Days
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ExportTimesheetCsv(int? year = null, int? month = null)
    {
        var employee = await CurrentUser.GetCurrentEmployeeAsync();
        var y = year ?? DateTime.UtcNow.Year;
        var m = month ?? DateTime.UtcNow.Month;

        var timesheetDto = await _attendanceService.GetMonthlyTimesheetAsync(employee.Id, y, m);

        var headers = new[]
        {
            "Date",
            "Day of Week",
            "Status",
            "Check In Time",
            "Check Out Time",
            "Hours Worked",
            "Late Check-In"
        };

        var rows = timesheetDto.Days.Select(day =>
        {
            string status;
            if (day.IsPresent) status = day.IsLate ? "Late" : "Present";
            else if (day.IsFuture) status = "Upcoming";
            else if (day.IsWeekday) status = "Absent";
            else status = "Weekend";

            return new object?[]
            {
                day.Date.ToString("yyyy-MM-dd"),
                day.Date.DayOfWeek.ToString(),
                status,
                day.CheckInTime.HasValue ? day.CheckInTime.Value.ToString("HH:mm:ss") : "—",
                day.CheckOutTime.HasValue ? day.CheckOutTime.Value.ToString("HH:mm:ss") : "—",
                day.HoursWorked.HasValue ? Math.Round(day.HoursWorked.Value, 2) : 0.0,
                day.IsLate ? "Yes" : "No"
            };
        });

        var csvBytes = CsvExportHelper.CreateCsvBytes(headers, rows);
        var sanitized = $"{employee.FirstName}_{employee.LastName}".Replace(" ", "_");
        var fileName = $"My_Timesheet_{sanitized}_{y}_{m:D2}.csv";
        return File(csvBytes, "text/csv; charset=utf-8", fileName);
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
