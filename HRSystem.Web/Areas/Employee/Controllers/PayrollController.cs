using HRSystem.Business.Interfaces.Services;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using EmployeeEntity = HRSystem.Data.Models.Employee;
using HRSystem.Web.Helpers;
using HRSystem.Web.ViewModels.Payroll;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.Employee.Controllers;

public class PayrollController : EmployeeBaseController
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
        await SetLayoutAsync("Payroll", "Search payslips...");
        return View(await BuildIndexModelAsync(page, detailId));
    }

    private async Task<EmployeePayrollIndexViewModel> BuildIndexModelAsync(int page, int? detailId = null)
    {
        var employee = await CurrentUser.GetCurrentEmployeeAsync();
        var paged = await _payrollService.GetByEmployeeAsync(employee.Id, page, PageSize);
        var departments = await HrDisplayHelper.LoadDepartmentsAsync(UnitOfWork);

        var payrolls = paged.Items
            .Select(p => MapListItem(p, employee, departments))
            .ToList();

        PayslipViewModel? payslipDetail = null;
        if (detailId.HasValue)
        {
            var payroll = paged.Items.FirstOrDefault(p => p.Id == detailId.Value)
                          ?? await UnitOfWork.Payrolls.GetByIdAsync(detailId.Value);

            if (payroll != null && payroll.EmployeeId == employee.Id)
                payslipDetail = await BuildPayslipAsync(payroll, employee, departments);
        }

        return new EmployeePayrollIndexViewModel
        {
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
        IReadOnlyDictionary<int, Department> departments)
    {
        var items = await UnitOfWork.PayrollItems.GetByPayrollIdAsync(payroll.Id);
        departments.TryGetValue(employee.DepartmentId, out var department);

        return new PayslipViewModel
        {
            Id = payroll.Id,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            DepartmentName = department?.Name ?? "Unknown",
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
        EmployeeEntity employee,
        IReadOnlyDictionary<int, Department> departments)
    {
        departments.TryGetValue(employee.DepartmentId, out var department);

        return new PayrollListItemViewModel
        {
            Id = payroll.Id,
            EmployeeId = payroll.EmployeeId,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            EmployeeInitials = HrDisplayHelper.GetInitials(employee.FirstName, employee.LastName),
            DepartmentName = department?.Name ?? "Unknown",
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
