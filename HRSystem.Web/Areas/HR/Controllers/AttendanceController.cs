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
    public async Task<IActionResult> Index(string? date = null, int? departmentId = null, int page = 1)
    {
        await SetLayoutAsync("Attendance", "Search attendance records...");
        var resolvedDate = !string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out var d)
            ? d
            : DateOnly.FromDateTime(DateTime.UtcNow);

        return View(await BuildIndexModelAsync(resolvedDate, departmentId, page));
    }

    private async Task<HrAttendanceIndexViewModel> BuildIndexModelAsync(DateOnly date, int? departmentId, int page)
    {
        var departmentsPaged = await UnitOfWork.Departments.GetActivePagedAsync(1, 500);
        var departments = departmentsPaged.Items
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.Name)
            .Select(d => (d.Id, d.Name))
            .ToList();

        var paged = await _attendanceService.GetReportAsync(date, departmentId, page, PageSize);

        var employees = new Dictionary<int, EmployeeEntity>();
        var departmentNames = departments.ToDictionary(x => x.Id, x => x.Name);

        foreach (var row in paged.Items)
        {
            var employee = await UnitOfWork.Employees.GetByIdAsync(row.EmployeeId);
            if (employee != null)
                employees[row.EmployeeId] = employee;
        }

        var records = paged.Items.Select(a =>
        {
            employees.TryGetValue(a.EmployeeId, out var employee);
            var label = AttendanceDisplayHelper.GetStatusLabel(a);
            var deptName = employee != null && departmentNames.TryGetValue(employee.DepartmentId, out var name)
                ? name
                : "—";

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
