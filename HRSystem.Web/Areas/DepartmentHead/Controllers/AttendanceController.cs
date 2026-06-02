using HRSystem.Business.DTOs.Attendance;
using HRSystem.Business.Exceptions;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using EmployeeEntity = HRSystem.Data.Models.Employee;
using HRSystem.Web.Helpers;
using HRSystem.Web.ViewModels.Attendance;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.DepartmentHead.Controllers;

public class AttendanceController : DepartmentHeadBaseController
{
    private const int TeamPageSize = 200;
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
    public async Task<IActionResult> Index(DateOnly? date = null, int? markEmployeeId = null)
    {
        await SetLayoutAsync("Attendance", "Search team attendance...");
        var manager = await CurrentUser.GetCurrentEmployeeAsync();
        var department = await UnitOfWork.Departments.GetByManagerIdAsync(manager.Id);
        if (department == null)
        {
            return View(new DeptHeadAttendanceIndexViewModel
            {
                DepartmentName = "No department assigned",
                Date = date ?? DateOnly.FromDateTime(DateTime.UtcNow)
            });
        }

        return View(await BuildIndexModelAsync(department, manager, date ?? DateOnly.FromDateTime(DateTime.UtcNow), markEmployeeId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Mark([Bind(Prefix = "MarkForm")] MarkTeamAttendanceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Attendance", "Search team attendance...");
            var manager = await CurrentUser.GetCurrentEmployeeAsync();
            var department = await UnitOfWork.Departments.GetByManagerIdAsync(manager.Id);
            if (department == null)
                return RedirectToAction(nameof(Index));

            return View("Index", await BuildIndexModelAsync(department, manager, model.Date, markEmployeeId: model.EmployeeId, markForm: model));
        }

        try
        {
            await _attendanceService.MarkTeamAttendanceAsync(new MarkTeamAttendanceDto
            {
                EmployeeId = model.EmployeeId,
                Date = model.Date,
                CheckInTime = model.CheckInTime,
                CheckOutTime = model.CheckOutTime,
                Notes = model.Notes
            });

            TempData["Success"] = "Attendance updated successfully.";
            return RedirectToAction(nameof(Index), new { date = model.Date.ToString("yyyy-MM-dd") });
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("Attendance", "Search team attendance...");
            var manager = await CurrentUser.GetCurrentEmployeeAsync();
            var department = await UnitOfWork.Departments.GetByManagerIdAsync(manager.Id);
            if (department == null)
                return RedirectToAction(nameof(Index));

            ModalValidationHelper.AddFormErrors(ModelState, "MarkForm", ex.Message);
            return View("Index", await BuildIndexModelAsync(department, manager, model.Date, markEmployeeId: model.EmployeeId, markForm: model));
        }
    }

    private async Task<DeptHeadAttendanceIndexViewModel> BuildIndexModelAsync(
        Department department,
        EmployeeEntity manager,
        DateOnly date,
        int? markEmployeeId = null,
        MarkTeamAttendanceViewModel? markForm = null)
    {
        var employeesPage = await UnitOfWork.Employees.GetByDepartmentPagedAsync(department.Id, 1, TeamPageSize);
        var employees = employeesPage.Items
            .Where(e => e.IsActive && !e.IsDeleted && !e.IsHR)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToList();

        var team = new List<TeamAttendanceRowViewModel>();
        foreach (var employee in employees)
        {
            var attendance = await UnitOfWork.Attendances.GetByEmployeeAndDateAsync(employee.Id, date);
            var label = attendance != null ? AttendanceDisplayHelper.GetStatusLabel(attendance) : "Absent";

            team.Add(new TeamAttendanceRowViewModel
            {
                EmployeeId = employee.Id,
                EmployeeName = TaskDisplayHelper.GetFullName(employee),
                EmployeeInitials = TaskDisplayHelper.GetInitials(employee),
                CheckInTime = attendance?.CheckInTime,
                CheckOutTime = attendance?.CheckOutTime,
                Notes = attendance?.Notes,
                StatusLabel = label,
                IsSelf = employee.Id == manager.Id
            });
        }

        MarkTeamAttendanceViewModel? resolvedMarkForm = null;
        if (markEmployeeId.HasValue || markForm != null)
        {
            var empId = markForm?.EmployeeId ?? markEmployeeId!.Value;
            var targetEmployee = employees.FirstOrDefault(e => e.Id == empId);
            if (targetEmployee != null)
            {
                var existing = await UnitOfWork.Attendances.GetByEmployeeAndDateAsync(empId, date);
                resolvedMarkForm = markForm ?? new MarkTeamAttendanceViewModel
                {
                    EmployeeId = empId,
                    EmployeeName = TaskDisplayHelper.GetFullName(targetEmployee),
                    Date = date,
                    CheckInTime = existing?.CheckInTime,
                    CheckOutTime = existing?.CheckOutTime,
                    Notes = existing?.Notes
                };
            }
        }

        return new DeptHeadAttendanceIndexViewModel
        {
            DepartmentName = department.Name,
            Date = date,
            Team = team,
            MarkForm = resolvedMarkForm
        };
    }
}
