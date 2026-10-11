using HRSystem.Business.DTOs.Payroll;
using HRSystem.Business.Exceptions;
using HRSystem.Business.Helpers;
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
    private const int PageSize = PaginationDefaults.DefaultPageSize;

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
        string? search = null,
        bool showProcess = false,
        bool showBatchProcess = false)
    {
        await SetLayoutAsync("Payroll", "Search payroll records...");
        return View(await BuildIndexModelAsync(page, departmentId, month, year, status, showProcess, showBatchProcess: showBatchProcess, search: search));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BatchProcess([Bind(Prefix = "BatchProcessForm")] BatchProcessPayrollViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Payroll", "Search payroll records...");
            return View("Index", await BuildIndexModelAsync(1, showBatchProcess: true, batchProcessForm: model));
        }

        try
        {
            var result = await _payrollService.BatchProcessAsync(new BatchProcessPayrollDto
            {
                Month = model.Month,
                Year = model.Year,
                DepartmentId = model.DepartmentId
            });

            TempData["Success"] = $"Bulk payroll generated {result.CreatedCount} draft(s) for {PayrollDisplayHelper.GetMonthName(model.Month)} {model.Year}. ({result.SkippedCount} skipped as already existing).";
            return RedirectToAction(nameof(Index), new { month = model.Month, year = model.Year, departmentId = model.DepartmentId });
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("Payroll", "Search payroll records...");
            ModalValidationHelper.AddFormErrors(ModelState, "BatchProcessForm", ex.Message);
            return View("Index", await BuildIndexModelAsync(1, showBatchProcess: true, batchProcessForm: model));
        }
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

    [HttpGet]
    public async Task<IActionResult> Payslip(int id)
    {
        await SetLayoutAsync("Payroll", "Search payroll records...");
        var payroll = await UnitOfWork.Payrolls.GetByIdAsync(id);
        if (payroll == null)
            return NotFound();

        var employee = await UnitOfWork.Employees.GetByIdAsync(payroll.EmployeeId);
        if (employee == null)
            return NotFound();

        var departments = await HrDisplayHelper.LoadDepartmentsAsync(UnitOfWork);
        departments.TryGetValue(employee.DepartmentId, out var department);
        var items = await UnitOfWork.PayrollItems.GetByPayrollIdAsync(payroll.Id);

        var model = new PayslipViewModel
        {
            Id = payroll.Id,
            EmployeeId = employee.Id,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            EmployeeCode = $"EMP-{employee.Id:D4}",
            DepartmentName = department?.Name ?? "—",
            JobTitle = employee.IsHR ? "HR Specialist" : "Staff Member",
            Email = employee.Email ?? "—",
            JoinDate = employee.CreatedAt,
            ProcessedAt = payroll.ProcessedAt,
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

        return View("~/Areas/Employee/Views/Payroll/Payslip.cshtml", model);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkApprove(int? month, int? year, int? departmentId)
    {
        try
        {
            var result = await _payrollService.BulkApproveAsync(month, year, departmentId);
            TempData["Success"] = $"Approved {result.UpdatedCount} payroll record(s). {result.SkippedCount} skipped.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { month, year, departmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkMarkPaid(int? month, int? year, int? departmentId)
    {
        try
        {
            var result = await _payrollService.BulkMarkPaidAsync(month, year, departmentId);
            TempData["Success"] = $"Marked {result.UpdatedCount} payroll record(s) as paid. Payslips are being dispatched in the background.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { month, year, departmentId });
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(
        int? departmentId = null,
        int? month = null,
        int? year = null,
        PayrollStatus? status = null,
        string? search = null)
    {
        var paged = await UnitOfWork.Payrolls.GetFilteredPagedAsync(departmentId, month, year, status, page: 1, pageSize: 10000, search: search);
        var payrolls = paged.Items;

        var employeeIds = payrolls.Select(p => p.EmployeeId).Distinct().ToList();
        var employeeMap = await UnitOfWork.Employees.GetByIdsAsync(employeeIds);
        var departmentsMap = await HrDisplayHelper.LoadDepartmentsAsync(UnitOfWork);

        var headers = new[]
        {
            "Payroll ID",
            "Employee ID",
            "Employee Name",
            "Email",
            "Department",
            "Period",
            "Year",
            "Month",
            "Status",
            "Base Salary",
            "Total Bonus",
            "Total Deduction",
            "Net Salary",
            "Processed Date"
        };

        var rows = payrolls.Select(p =>
        {
            employeeMap.TryGetValue(p.EmployeeId, out var emp);
            string deptName = "—";
            if (emp != null && departmentsMap.TryGetValue(emp.DepartmentId, out var dept))
            {
                deptName = dept.Name;
            }

            var periodLabel = $"{PayrollDisplayHelper.GetMonthName(p.Month)} {p.Year}";
            var empName = emp != null ? $"{emp.FirstName} {emp.LastName}" : $"Employee #{p.EmployeeId}";
            var empEmail = emp?.Email ?? string.Empty;

            return new object?[]
            {
                p.Id,
                p.EmployeeId,
                empName,
                empEmail,
                deptName,
                periodLabel,
                p.Year,
                p.Month,
                p.Status.ToString(),
                p.BaseSalary,
                p.TotalBonus,
                p.TotalDeduction,
                p.NetSalary,
                p.ProcessedAt.HasValue ? p.ProcessedAt.Value.ToString("yyyy-MM-dd HH:mm") : "—"
            };
        });

        var csvBytes = CsvExportHelper.CreateCsvBytes(headers, rows);
        var fileName = $"Payroll_Export_{year ?? DateTime.UtcNow.Year}_{(month.HasValue ? month.Value.ToString("D2") : "All")}_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
        return File(csvBytes, "text/csv; charset=utf-8", fileName);
    }

    private async Task<HrPayrollIndexViewModel> BuildIndexModelAsync(
        int page,
        int? departmentId = null,
        int? month = null,
        int? year = null,
        PayrollStatus? status = null,
        bool showProcess = false,
        bool showBatchProcess = false,
        ProcessPayrollViewModel? processForm = null,
        BatchProcessPayrollViewModel? batchProcessForm = null,
        string? search = null)
    {
        var departments = await HrDisplayHelper.LoadDepartmentsAsync(UnitOfWork);
        var employees = await HrDisplayHelper.LoadEmployeesAsync(UnitOfWork);
        var paged = await _payrollService.GetFilteredAsync(departmentId, month, year, status, page, PageSize, search);

        var payrolls = paged.Items
            .Select(p => MapListItem(p, employees, departments))
            .ToList();

        var statsPage = await _payrollService.GetFilteredAsync(departmentId, month, year, null, 1, 500, search);

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
            BatchProcessForm = batchProcessForm ?? new BatchProcessPayrollViewModel(),
            DepartmentFilter = departmentId,
            MonthFilter = month,
            YearFilter = year,
            StatusFilter = status,
            Search = search,
            ShowProcessModal = showProcess,
            ShowBatchProcessModal = showBatchProcess,
            TotalNetForPeriod = statsPage.Items.Sum(p => p.NetSalary),
            PendingApprovalCount = statsPage.Items.Count(p => p.Status == PayrollStatus.Draft),
            ApprovedPendingPayCount = statsPage.Items.Count(p => p.Status == PayrollStatus.Approved),
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
