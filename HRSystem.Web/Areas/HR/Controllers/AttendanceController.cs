using HRSystem.Business.Interfaces.Services;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using EmployeeEntity = HRSystem.Data.Models.Employee;
using HRSystem.Web.Helpers;
using HRSystem.Web.ViewModels.Attendance;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.HR.Controllers;

public class AttendanceController : HRBaseController
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
    public async Task<IActionResult> Index(string? date = null, int? departmentId = null, string? search = null, int page = 1)
    {
        await SetLayoutAsync("Attendance", "Search attendance records...");
        var resolvedDate = !string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out var d)
            ? d
            : DateOnly.FromDateTime(DateTime.UtcNow);

        return View(await BuildIndexModelAsync(resolvedDate, departmentId, search, page));
    }

    [HttpGet]
    public async Task<IActionResult> Monthly(int? year = null, int? month = null, int? departmentId = null, string? search = null)
    {
        await SetLayoutAsync("Attendance", "Search attendance records...");
        var y = year ?? DateTime.UtcNow.Year;
        var m = month ?? DateTime.UtcNow.Month;

        var departmentsPaged = await UnitOfWork.Departments.GetActivePagedAsync(1, 500);
        var departments = departmentsPaged.Items
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.Name)
            .Select(d => (d.Id, d.Name))
            .ToList();

        var employees = await UnitOfWork.Employees.GetActiveForPayrollAsync(departmentId);
        var timesheets = await _attendanceService.GetMonthlyDepartmentTimesheetsAsync(departmentId, y, m);

        var byEmp = timesheets.ToDictionary(t => t.EmployeeId);
        var departmentsMap = await HrDisplayHelper.LoadDepartmentsAsync(UnitOfWork);

        var records = new List<HrMonthlyAttendanceRowViewModel>();
        foreach (var emp in employees)
        {
            var fullName = $"{emp.FirstName} {emp.LastName}";
            if (!string.IsNullOrWhiteSpace(search) &&
                !fullName.Contains(search, StringComparison.OrdinalIgnoreCase) &&
                !(emp.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                continue;
            }

            departmentsMap.TryGetValue(emp.DepartmentId, out var dept);
            byEmp.TryGetValue(emp.Id, out var ts);

            records.Add(new HrMonthlyAttendanceRowViewModel
            {
                EmployeeId = emp.Id,
                EmployeeName = fullName,
                EmployeeInitials = TaskDisplayHelper.GetInitials(emp),
                DepartmentName = dept?.Name ?? "—",
                JobTitle = emp.IsHR ? "HR Specialist" : "Staff Member",
                DaysPresent = ts?.DaysPresent ?? 0,
                WeekdayAbsences = ts?.WeekdayAbsences ?? 0,
                LateCheckIns = ts?.LateCheckIns ?? 0,
                TotalHours = ts?.TotalHours ?? 0
            });
        }

        var start = new DateOnly(y, m, 1);
        var end = start.AddMonths(1).AddDays(-1);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var workDaysTotal = 0;
        var workDaysToDate = 0;
        for (var dt = start; dt <= end; dt = dt.AddDays(1))
        {
            if (dt.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
            {
                workDaysTotal++;
                if (dt <= today)
                    workDaysToDate++;
            }
        }

        var model = new HrMonthlyAttendanceIndexViewModel
        {
            SelectedYear = y,
            SelectedMonth = m,
            DepartmentFilter = departmentId,
            Search = search,
            Departments = departments,
            Records = records,
            TotalActiveEmployees = employees.Count,
            TotalWorkDaysInMonth = workDaysTotal,
            WorkDaysToDate = workDaysToDate
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Timesheet(int employeeId, int? year = null, int? month = null)
    {
        await SetLayoutAsync("Attendance", "Search attendance records...");
        var employee = await UnitOfWork.Employees.GetByIdAsync(employeeId);
        if (employee == null)
            return NotFound();

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
            JobTitle = employee.IsHR ? "HR Specialist" : "Staff Member",
            Year = y,
            Month = m,
            DaysPresent = timesheetDto.DaysPresent,
            WeekdayAbsences = timesheetDto.WeekdayAbsences,
            LateCheckIns = timesheetDto.LateCheckIns,
            TotalHours = timesheetDto.TotalHours,
            Days = timesheetDto.Days
        };

        return View("~/Areas/Employee/Views/Attendance/Timesheet.cshtml", model);
    }

    private async Task<HrAttendanceIndexViewModel> BuildIndexModelAsync(DateOnly date, int? departmentId, string? search, int page)
    {
        var departmentsPaged = await UnitOfWork.Departments.GetActivePagedAsync(1, 500);
        var departments = departmentsPaged.Items
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.Name)
            .Select(d => (d.Id, d.Name))
            .ToList();

        var paged = await _attendanceService.GetReportAsync(date, departmentId, page, PageSize, search);

        var records = paged.Items.Select(a =>
        {
            var employee = a.Employee;
            var label = AttendanceDisplayHelper.GetStatusLabel(a);
            var deptName = employee?.Department?.Name ?? "—";

            return new HrAttendanceRowViewModel
            {
                EmployeeId = a.EmployeeId,
                EmployeeName = employee != null ? TaskDisplayHelper.GetFullName(employee) : "Unknown",
                EmployeeInitials = employee != null ? TaskDisplayHelper.GetInitials(employee) : "?",
                DepartmentName = deptName,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                StatusLabel = label,
                Notes = a.Notes
            };
        }).ToList();

        return new HrAttendanceIndexViewModel
        {
            Date = date,
            DepartmentId = departmentId,
            Departments = departments,
            Records = records,
            Page = paged.Page,
            TotalPages = paged.TotalPages,
            TotalCount = paged.TotalCount,
            PageSize = paged.PageSize
        };
    }
}
