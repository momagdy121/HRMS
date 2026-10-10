using HRSystem.Business.Interfaces.Services;
using HRSystem.Common.Enums;
using HRSystem.Data.Interfaces;
using HRSystem.Web.Helpers;
using HRSystem.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Mvc;
using EmployeeTaskStatus = HRSystem.Common.Enums.TaskStatus;

namespace HRSystem.Web.Areas.HR.Controllers;

public class DashboardController : HRBaseController
{
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;
    private readonly ILeaveService _leaveService;
    private readonly IPayrollService _payrollService;

    public DashboardController(
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IEmployeeService employeeService,
        IDepartmentService departmentService,
        ILeaveService leaveService,
        IPayrollService payrollService)
        : base(currentUser, unitOfWork)
    {
        _employeeService = employeeService;
        _departmentService = departmentService;
        _leaveService = leaveService;
        _payrollService = payrollService;
    }

    public async Task<IActionResult> Index()
    {
        await SetLayoutAsync("Dashboard");

        var employee = await CurrentUser.GetCurrentEmployeeAsync();
        var allEmployees = await HrDisplayHelper.LoadEmployeesAsync(UnitOfWork);
        var employees = await _employeeService.GetAllAsync(1, 1);
        var departments = await _departmentService.GetAllAsync(1, 1);
        var pendingLeave = await _leaveService.GetAllPendingAsync(1, 1);
        var payrolls = await _payrollService.GetAllAsync(1, 500);
        var now = DateTime.UtcNow;

        var recentLeaves = await _leaveService.GetFilteredAsync(null, 1, 3);
        var recentEmployees = await _employeeService.GetAllAsync(1, 3);
        var recentPayrolls = await _payrollService.GetAllAsync(1, 3);

        var activities = new List<DashboardActivityItemViewModel>();

        foreach (var l in recentLeaves.Items)
        {
            allEmployees.TryGetValue(l.EmployeeId, out var emp);
            var empName = emp != null ? $"{emp.FirstName} {emp.LastName}" : $"Employee #{l.EmployeeId}";
            activities.Add(new DashboardActivityItemViewModel
            {
                Title = $"{empName} requested {l.LeaveType} leave",
                Description = $"{l.StartDate:MMM dd} - {l.EndDate:MMM dd}. Status: {l.Status}.",
                TimeAgo = l.RequestDate.ToString("MMM dd"),
                Icon = "event_busy",
                IconColorClass = "bg-error-container text-error"
            });
        }

        foreach (var emp in recentEmployees.Items)
        {
            activities.Add(new DashboardActivityItemViewModel
            {
                Title = $"Employee {emp.FirstName} {emp.LastName} active",
                Description = $"Department: {emp.Department?.Name ?? "General"}. Hired on {emp.HireDate:MMM dd, yyyy}.",
                TimeAgo = emp.HireDate.ToString("MMM dd"),
                Icon = "person_add",
                IconColorClass = "bg-[#d1fae5] text-[#047857]"
            });
        }

        foreach (var pay in recentPayrolls.Items.Take(2))
        {
            activities.Add(new DashboardActivityItemViewModel
            {
                Title = $"{new DateTime(pay.Year, pay.Month, 1):MMMM yyyy} payroll record",
                Description = $"Net pay: {pay.NetSalary:C0}. Status: {pay.Status}.",
                TimeAgo = $"{pay.Month}/{pay.Year}",
                Icon = "payments",
                IconColorClass = "bg-primary-fixed/30 text-primary-container"
            });
        }

        var model = new HrDashboardViewModel
        {
            FirstName = employee.FirstName,
            TotalEmployees = employees.TotalCount,
            ActiveDepartments = departments.TotalCount,
            PendingLeaveRequests = pendingLeave.TotalCount,
            MonthlyPayrollTotal = payrolls.Items
                .Where(p => p.Month == now.Month && p.Year == now.Year)
                .Sum(p => p.NetSalary),
            RecentActivities = activities.Take(6).ToList()
        };

        return View(model);
    }
}
