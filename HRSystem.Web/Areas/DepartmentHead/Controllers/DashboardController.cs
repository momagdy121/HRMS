using HRSystem.Business.Interfaces.Services;
using HRSystem.Data.Interfaces;
using HRSystem.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Mvc;
using EmployeeTaskStatus = HRSystem.Common.Enums.TaskStatus;

namespace HRSystem.Web.Areas.DepartmentHead.Controllers;

public class DashboardController : DepartmentHeadBaseController
{
    private readonly IEmployeeService _employeeService;
    private readonly ILeaveService _leaveService;
    private readonly IAttendanceService _attendanceService;
    private readonly ITaskService _taskService;

    public DashboardController(
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IEmployeeService employeeService,
        ILeaveService leaveService,
        IAttendanceService attendanceService,
        ITaskService taskService)
        : base(currentUser, unitOfWork)
    {
        _employeeService = employeeService;
        _leaveService = leaveService;
        _attendanceService = attendanceService;
        _taskService = taskService;
    }

    public async Task<IActionResult> Index()
    {
        await SetLayoutAsync("Dashboard");
        var department = await GetManagedDepartmentAsync();
        return View(await BuildDashboardModelAsync(department));
    }

    public async Task<IActionResult> MyDepartment()
    {
        await SetLayoutAsync("MyDepartment", "Search team members...");
        var department = await GetManagedDepartmentAsync();
        ViewBag.DepartmentName = department.Name;
        return View(await BuildDashboardModelAsync(department));
    }

    private async Task<Data.Models.Department> GetManagedDepartmentAsync()
    {
        var employee = await CurrentUser.GetCurrentEmployeeAsync();
        return await UnitOfWork.Departments.GetByManagerIdAsync(employee.Id)
               ?? throw new InvalidOperationException("No department assigned to this manager.");
    }

    private async Task<DepartmentHeadDashboardViewModel> BuildDashboardModelAsync(Data.Models.Department department)
    {
        var employees = await _employeeService.GetByDepartmentAsync(department.Id, 1, 1);
        var pendingLeave = await _leaveService.GetPendingByDepartmentAsync(department.Id, 1, 1);
        var manager = await CurrentUser.GetCurrentEmployeeAsync();
        var tasks = await _taskService.GetByManagerAsync(manager.Id, 1, 200);

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var employeeIds = (await _employeeService.GetByDepartmentAsync(department.Id, 1, 500)).Items.Select(e => e.Id).ToHashSet();

        var presentCount = 0;
        foreach (var employeeId in employeeIds)
        {
            var records = await _attendanceService.GetByEmployeeAsync(employeeId, 1, 50);
            if (records.Items.Any(a =>
                    !a.IsDeleted
                    && a.Date.Month == now.Month
                    && a.Date.Year == now.Year
                    && a.CheckInTime.HasValue))
            {
                presentCount++;
            }
        }

        var totalEmployees = employees.TotalCount;
        var attendancePercent = totalEmployees == 0 ? 0 : (int)Math.Round(presentCount * 100.0 / totalEmployees);

        var overdueTasks = tasks.Items.Count(t =>
            t.Status != EmployeeTaskStatus.Completed
            && t.DueDate.HasValue
            && t.DueDate.Value < today);

        return new DepartmentHeadDashboardViewModel
        {
            DepartmentName = department.Name,
            ActiveEmployees = totalEmployees,
            PendingLeaveRequests = pendingLeave.TotalCount,
            PresentThisMonth = presentCount,
            AttendancePercent = attendancePercent,
            OverdueTasks = overdueTasks
        };
    }
}
