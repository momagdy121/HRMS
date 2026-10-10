using HRSystem.Business.Interfaces.Services;
using HRSystem.Common.Enums;
using HRSystem.Data.Interfaces;
using HRSystem.Web.Helpers;
using HRSystem.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Mvc;
using EmployeeTaskStatus = HRSystem.Common.Enums.TaskStatus;

namespace HRSystem.Web.Areas.Employee.Controllers;

public class DashboardController : EmployeeBaseController
{
    private readonly ITaskService _taskService;
    private readonly ILeaveService _leaveService;
    private readonly IAttendanceService _attendanceService;
    private readonly IPayrollService _payrollService;

    public DashboardController(
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        ITaskService taskService,
        ILeaveService leaveService,
        IAttendanceService attendanceService,
        IPayrollService payrollService)
        : base(currentUser, unitOfWork)
    {
        _taskService = taskService;
        _leaveService = leaveService;
        _attendanceService = attendanceService;
        _payrollService = payrollService;
    }

    public async Task<IActionResult> Index()
    {
        await SetLayoutAsync("Dashboard");

        var employee = await CurrentUser.GetCurrentEmployeeAsync();
        var tasks = await _taskService.GetByEmployeeAsync(employee.Id, 1, 100);
        var balance = await _leaveService.GetBalanceAsync(employee.Id, DateTime.UtcNow.Year, LeaveType.Annual);
        var attendance = await _attendanceService.GetByEmployeeAsync(employee.Id, 1, 100);
        var payrolls = await _payrollService.GetByEmployeeAsync(employee.Id, 1, 1);

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var dueSoonCutoff = today.AddDays(7);

        var activeTasks = tasks.Items.Count(t => t.Status != EmployeeTaskStatus.Completed);
        var dueSoon = tasks.Items.Count(t =>
            t.Status != EmployeeTaskStatus.Completed
            && t.DueDate.HasValue
            && t.DueDate.Value <= dueSoonCutoff);

        var daysPresent = attendance.Items.Count(a =>
            !a.IsDeleted
            && a.Date.Month == now.Month
            && a.Date.Year == now.Year
            && a.CheckInTime.HasValue);

        var recentPayrolls = await _payrollService.GetByEmployeeAsync(employee.Id, 1, 5);
        var payslips = recentPayrolls.Items.Select(p => new EmployeeDashboardPayslipViewModel
        {
            Id = p.Id,
            Period = $"{new DateTime(p.Year, p.Month, 1):MMMM yyyy}",
            PaymentDate = p.Status == PayrollStatus.Paid
                ? $"Paid {new DateTime(p.Year, p.Month, DateTime.DaysInMonth(p.Year, p.Month)):MMM dd}"
                : p.Status.ToString(),
            NetSalary = p.NetSalary
        }).ToList();

        var department = await UnitOfWork.Departments.GetByIdAsync(employee.DepartmentId);
        var deptEmployees = await UnitOfWork.Employees.GetByDepartmentPagedAsync(employee.DepartmentId, 1, 10);
        var todayAttendances = await UnitOfWork.Attendances.GetReportPagedAsync(today, employee.DepartmentId, 1, 50);
        var todayAttendanceMap = todayAttendances.Items.ToDictionary(a => a.EmployeeId);

        var teamMembers = deptEmployees.Items
            .Where(e => e.IsActive && !e.IsDeleted)
            .Select(e =>
            {
                var isManager = department?.ManagerId == e.Id;
                var roleName = isManager ? "Department Head" : (e.IsHR ? "HR Specialist" : "Team Member");
                var hasCheckedIn = todayAttendanceMap.TryGetValue(e.Id, out var att) && att.CheckInTime.HasValue;

                return new EmployeeDashboardTeamMemberViewModel
                {
                    Id = e.Id,
                    Name = $"{e.FirstName} {e.LastName}",
                    Role = roleName,
                    Initials = HrDisplayHelper.GetInitials(e.FirstName, e.LastName),
                    IsPresentToday = hasCheckedIn,
                    StatusText = hasCheckedIn ? "Present" : "Offline"
                };
            }).ToList();

        var model = new EmployeeDashboardViewModel
        {
            FirstName = employee.FirstName,
            DepartmentName = department?.Name ?? "Department",
            ActiveTasks = activeTasks,
            TasksDueSoon = dueSoon,
            LeaveBalanceDays = balance != null ? balance.TotalDays - balance.UsedDays : 0,
            DaysPresentThisMonth = daysPresent,
            LastNetSalary = payslips.FirstOrDefault()?.NetSalary ?? 0,
            RecentPayslips = payslips,
            TeamMembers = teamMembers
        };

        return View(model);
    }
}
