using HRSystem.Business.DTOs.Payroll;
using HRSystem.Business.Exceptions;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Common.Enums;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using EmployeeEntity = HRSystem.Data.Models.Employee;
using DepartmentEntity = HRSystem.Data.Models.Department;
using HRSystem.Web.Helpers;
using HRSystem.Web.ViewModels.Payroll;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.HR.Controllers;

public class PayrollController : HRBaseController
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
    public async Task<IActionResult> Index(
        int page = 1,
        int? departmentId = null,
        int? month = null,
        int? year = null,
        PayrollStatus? status = null,
        bool showProcess = false)
    {
        await SetLayoutAsync("Payroll", "Search payroll records...");
        return View(await BuildIndexModelAsync(page, departmentId, month, year, status, showProcess));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Process([Bind(Prefix = "ProcessForm")] ProcessPayrollViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Payroll", "Search payroll records...");
            return View("Index", await BuildIndexModelAsync(1, showProcess: true, processForm: model));
        }

        try
        {
            var payroll = await _payrollService.ProcessAsync(new ProcessPayrollDto
            {
                EmployeeId = model.EmployeeId,
                Month = model.Month,
                Year = model.Year,
                BaseSalary = 0
            });

            TempData["Success"] = "Payroll draft created successfully.";
            return RedirectToAction(nameof(Detail), new { id = payroll.Id });
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("Payroll", "Search payroll records...");
            ModalValidationHelper.AddFormErrors(ModelState, "ProcessForm", ex.Message);
            return View("Index", await BuildIndexModelAsync(1, showProcess: true, processForm: model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id, int? editItemId = null, int? removeItemId = null)
    {
        await SetLayoutAsync("Payroll", "Search payroll records...");
        var model = await BuildDetailModelAsync(id, editItemId: editItemId, removeItemId: removeItemId);
        if (model == null)
            return RedirectToAction(nameof(Index));

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem([Bind(Prefix = "AddItemForm")] AddPayrollItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Payroll", "Search payroll records...");
            var detail = await BuildDetailModelAsync(model.PayrollId, model);
            if (detail == null)
                return RedirectToAction(nameof(Index));

            return View("Detail", detail);
        }

        try
        {
            await _payrollService.AddPayrollItemAsync(new AddPayrollItemDto
            {
                PayrollId = model.PayrollId,
                ItemType = model.ItemType,
                Description = model.Description,
                Amount = model.Amount
            });

            TempData["Success"] = "Payroll item added successfully.";
            return RedirectToAction(nameof(Detail), new { id = model.PayrollId });
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("Payroll", "Search payroll records...");
            var detail = await BuildDetailModelAsync(model.PayrollId, model);
            if (detail == null)
                return RedirectToAction(nameof(Index));

            ModalValidationHelper.AddFormErrors(ModelState, "AddItemForm", ex.Message);
            return View("Detail", detail);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditItem([Bind(Prefix = "EditItemForm")] EditPayrollItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Payroll", "Search payroll records...");
            var detail = await BuildDetailModelAsync(model.PayrollId, editItemForm: model);
            if (detail == null)
                return RedirectToAction(nameof(Index));

            return View("Detail", detail);
        }

        try
        {
            await _payrollService.UpdatePayrollItemAsync(new EditPayrollItemDto
            {
                Id = model.Id,
                PayrollId = model.PayrollId,
                ItemType = model.ItemType,
                Description = model.Description,
                Amount = model.Amount
            });

            TempData["Success"] = "Payroll item updated successfully.";
            return RedirectToAction(nameof(Detail), new { id = model.PayrollId });
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("Payroll", "Search payroll records...");
            var detail = await BuildDetailModelAsync(model.PayrollId, editItemForm: model);
            if (detail == null)
                return RedirectToAction(nameof(Index));

            ModalValidationHelper.AddFormErrors(ModelState, "EditItemForm", ex.Message);
            return View("Detail", detail);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int id, int payrollId)
    {
        try
        {
            await _payrollService.RemovePayrollItemAsync(id);
            TempData["Success"] = "Payroll item removed successfully.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Detail), new { id = payrollId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            await _payrollService.UpdateStatusAsync(id, PayrollStatus.Approved);
            TempData["Success"] = "Payroll approved successfully.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkPaid(int id)
    {
        try
        {
            await _payrollService.UpdateStatusAsync(id, PayrollStatus.Paid);
            TempData["Success"] = "Payroll marked as paid.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Detail), new { id });
    }

    private async Task<HrPayrollIndexViewModel> BuildIndexModelAsync(
        int page,
        int? departmentId = null,
        int? month = null,
        int? year = null,
        PayrollStatus? status = null,
        bool showProcess = false,
        ProcessPayrollViewModel? processForm = null)
    {
        var departments = await HrDisplayHelper.LoadDepartmentsAsync(UnitOfWork);
        var employees = await HrDisplayHelper.LoadEmployeesAsync(UnitOfWork);
        var paged = await _payrollService.GetFilteredAsync(departmentId, month, year, status, page, PageSize);

        var payrolls = paged.Items
            .Select(p => MapListItem(p, employees, departments))
            .ToList();

        var statsPage = await _payrollService.GetFilteredAsync(departmentId, month, year, null, 1, 500);

        return new HrPayrollIndexViewModel
        {
            Payrolls = payrolls,
            Departments = departments.Values
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentFilterOptionViewModel { Id = d.Id, Name = d.Name })
                .ToList(),
            Employees = employees.Values
                .Where(e => e.IsActive && !e.IsDeleted)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .Select(e => new EmployeeFilterOptionViewModel
                {
                    Id = e.Id,
                    Name = $"{e.FirstName} {e.LastName}"
                })
                .ToList(),
            ProcessForm = processForm ?? new ProcessPayrollViewModel(),
            DepartmentFilter = departmentId,
            MonthFilter = month,
            YearFilter = year,
            StatusFilter = status,
            ShowProcessModal = showProcess,
            TotalNetForPeriod = statsPage.Items.Sum(p => p.NetSalary),
            PendingApprovalCount = statsPage.Items.Count(p => p.Status == PayrollStatus.Draft),
            Page = paged.Page,
            TotalPages = paged.TotalPages,
            TotalCount = paged.TotalCount,
            PageSize = paged.PageSize
        };
    }

    private async Task<HrPayrollDetailViewModel?> BuildDetailModelAsync(
        int id,
        AddPayrollItemViewModel? addItemForm = null,
        EditPayrollItemViewModel? editItemForm = null,
        int? editItemId = null,
        int? removeItemId = null)
    {
        var payroll = await UnitOfWork.Payrolls.GetByIdAsync(id);
        if (payroll == null)
            return null;

        var employees = await HrDisplayHelper.LoadEmployeesAsync(UnitOfWork);
        var departments = await HrDisplayHelper.LoadDepartmentsAsync(UnitOfWork);
        employees.TryGetValue(payroll.EmployeeId, out var employee);

        var items = await UnitOfWork.PayrollItems.GetByPayrollIdAsync(id);
        var canManageItems = payroll.Status == PayrollStatus.Draft;

        EditPayrollItemViewModel? resolvedEditForm = editItemForm;
        if (resolvedEditForm == null && editItemId.HasValue)
        {
            var item = items.FirstOrDefault(i => i.Id == editItemId.Value);
            if (item != null && canManageItems)
            {
                resolvedEditForm = new EditPayrollItemViewModel
                {
                    Id = item.Id,
                    PayrollId = payroll.Id,
                    ItemType = item.ItemType,
                    Description = item.Description,
                    Amount = item.Amount
                };
            }
        }

        RemovePayrollItemViewModel? removeTarget = null;
        if (removeItemId.HasValue)
        {
            var item = items.FirstOrDefault(i => i.Id == removeItemId.Value);
            if (item != null && canManageItems)
            {
                removeTarget = new RemovePayrollItemViewModel
                {
                    Id = item.Id,
                    PayrollId = payroll.Id,
                    ItemType = item.ItemType,
                    Description = item.Description,
                    Amount = item.Amount
                };
            }
        }

        return new HrPayrollDetailViewModel
        {
            Id = payroll.Id,
            EmployeeId = payroll.EmployeeId,
            EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown",
            DepartmentName = employee != null && departments.TryGetValue(employee.DepartmentId, out var dept)
                ? dept.Name
                : "Unknown",
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
            }).ToList(),
            AddItemForm = addItemForm ?? new AddPayrollItemViewModel { PayrollId = payroll.Id },
            EditItemForm = resolvedEditForm,
            RemoveItemTarget = removeTarget,
            CanManageItems = canManageItems,
            CanAddItems = canManageItems,
            CanApprove = payroll.Status == PayrollStatus.Draft,
            CanMarkPaid = payroll.Status == PayrollStatus.Approved
        };
    }

    private static PayrollListItemViewModel MapListItem(
        Payroll payroll,
        IReadOnlyDictionary<int, EmployeeEntity> employees,
        IReadOnlyDictionary<int, DepartmentEntity> departments)
    {
        employees.TryGetValue(payroll.EmployeeId, out var employee);
        var departmentName = employee != null && departments.TryGetValue(employee.DepartmentId, out var dept)
            ? dept.Name
            : "Unknown";

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
