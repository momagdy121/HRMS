using HRSystem.Business.Interfaces.Services;
using HRSystem.Common.Enums;
using HRSystem.Data.Interfaces;
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
        var employees = await _employeeService.GetAllAsync(1, 1);
        var departments = await _departmentService.GetAllAsync(1, 1);
        var pendingLeave = await _leaveService.GetAllPendingAsync(1, 1);
        var payrolls = await _payrollService.GetAllAsync(1, 500);
        var now = DateTime.UtcNow;

        var model = new HrDashboardViewModel
        {
            FirstName = employee.FirstName,
            TotalEmployees = employees.TotalCount,
            ActiveDepartments = departments.TotalCount,
            PendingLeaveRequests = pendingLeave.TotalCount,
            MonthlyPayrollTotal = payrolls.Items
                .Where(p => p.Month == now.Month && p.Year == now.Year)
                .Sum(p => p.NetSalary)
        };

        return View(model);
    }
}
