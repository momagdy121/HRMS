using HRSystem.Business.Interfaces.Services;
using HRSystem.Common.Enums;
using HRSystem.Data.Interfaces;
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

        var model = new EmployeeDashboardViewModel
        {
            FirstName = employee.FirstName,
            ActiveTasks = activeTasks,
            TasksDueSoon = dueSoon,
            LeaveBalanceDays = balance != null ? balance.TotalDays - balance.UsedDays : 0,
            DaysPresentThisMonth = daysPresent,
            LastNetSalary = payrolls.Items.FirstOrDefault()?.NetSalary ?? 0
        };

        return View(model);
    }
}
