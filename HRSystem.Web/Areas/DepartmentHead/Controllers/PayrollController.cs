using HRSystem.Business.Interfaces.Services;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using HRSystem.Web.Helpers;
using HRSystem.Web.ViewModels.Payroll;
using Microsoft.AspNetCore.Mvc;
using EmployeeEntity = HRSystem.Data.Models.Employee;

namespace HRSystem.Web.Areas.DepartmentHead.Controllers;

public class PayrollController : DepartmentHeadBaseController
{
    private const int PageSize = 10;

    private readonly IPayrollService _payrollService;

    public PayrollController(
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IPayrollService payrollService)
        : base(currentUser, unitOfWork)
    {
        _payrollService = payrollService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int? detailId = null)
    {
        await SetLayoutAsync("Payroll", "Search department payroll...");
        return View(await BuildIndexModelAsync(page, detailId));
    }

    private async Task<DeptHeadPayrollIndexViewModel> BuildIndexModelAsync(int page, int? detailId = null)
    {
        var manager = await CurrentUser.GetCurrentEmployeeAsync();
        var department = await UnitOfWork.Departments.GetByManagerIdAsync(manager.Id)
                         ?? throw new InvalidOperationException("Department not found for manager.");

        var paged = await _payrollService.GetByDepartmentAsync(department.Id, page, PageSize);
        var employees = await HrDisplayHelper.LoadEmployeesAsync(UnitOfWork);

        var payrolls = paged.Items
            .Select(p => MapListItem(p, employees, department.Name))
            .ToList();

        PayslipViewModel? payslipDetail = null;
        if (detailId.HasValue)
        {
            var payroll = paged.Items.FirstOrDefault(p => p.Id == detailId.Value)
                          ?? await UnitOfWork.Payrolls.GetByIdAsync(detailId.Value);

            if (payroll != null
                && employees.TryGetValue(payroll.EmployeeId, out var employee)
                && employee.DepartmentId == department.Id)
            {
                payslipDetail = await BuildPayslipAsync(payroll, employee, department.Name);
            }
        }

        return new DeptHeadPayrollIndexViewModel
        {
            DepartmentName = department.Name,
            Payrolls = payrolls,
            PayslipDetail = payslipDetail,
            Page = paged.Page,
            TotalPages = paged.TotalPages,
            TotalCount = paged.TotalCount,
            PageSize = paged.PageSize
        };
    }

    private async Task<PayslipViewModel> BuildPayslipAsync(
        Payroll payroll,
        EmployeeEntity employee,
        string departmentName)
    {
        var items = await UnitOfWork.PayrollItems.GetByPayrollIdAsync(payroll.Id);

        return new PayslipViewModel
        {
            Id = payroll.Id,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            DepartmentName = departmentName,
            Month = payroll.Month,
            Year = payroll.Year,
            BaseSalary = payroll.BaseSalary,
            TotalBonus = payroll.TotalBonus,
            TotalDeduction = payroll.TotalDeduction,
            NetSalary = payroll.NetSalary,
            Status = payroll.Status,
            Items = items.Select(i => new PayrollItemRowViewModel
            {
                Id = i.Id,
                ItemType = i.ItemType,
                Description = i.Description,
                Amount = i.Amount
            }).ToList()
        };
    }

    private static PayrollListItemViewModel MapListItem(
        Payroll payroll,
        IReadOnlyDictionary<int, EmployeeEntity> employees,
        string departmentName)
    {
        employees.TryGetValue(payroll.EmployeeId, out var employee);

        return new PayrollListItemViewModel
        {
            Id = payroll.Id,
            EmployeeId = payroll.EmployeeId,
            EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown",
            EmployeeInitials = employee != null
                ? HrDisplayHelper.GetInitials(employee.FirstName, employee.LastName)
                : "?",
            DepartmentName = departmentName,
            Month = payroll.Month,
            Year = payroll.Year,
            BaseSalary = payroll.BaseSalary,
            TotalBonus = payroll.TotalBonus,
            TotalDeduction = payroll.TotalDeduction,
            NetSalary = payroll.NetSalary,
            Status = payroll.Status
        };
    }
}
