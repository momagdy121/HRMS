using HRSystem.Business.DTOs.Employees;
using HRSystem.Business.Exceptions;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using HRSystem.Web.Helpers;
using HRSystem.Web.ViewModels.HR;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.HR.Controllers;

public class EmployeeController : HRBaseController
{
    private const int PageSize = 10;
    private readonly IEmployeeService _employeeService;

    public EmployeeController(
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IEmployeeService employeeService)
        : base(currentUser, unitOfWork)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int? departmentId = null, int? editId = null, bool showCreate = false)
    {
        await SetLayoutAsync("Employees", "Search employees...");
        return View(await BuildIndexModelAsync(page, departmentId, editId, showCreate));
    }

    [HttpGet]
    public async Task<IActionResult> Deleted(int page = 1)
    {
        await SetLayoutAsync("Employees", "Search deleted employees...");

        var departments = await HrDisplayHelper.LoadDepartmentsAsync(UnitOfWork);
        var paged = await _employeeService.GetDeletedAsync(page, PageSize);

        return View("Index", new EmployeeIndexViewModel
        {
            Employees = paged.Items.Select(e => MapListItem(e, departments, new Dictionary<int, bool>())).ToList(),
            Departments = ToDepartmentOptions(departments),
            Page = paged.Page,
            TotalPages = paged.TotalPages,
            TotalCount = paged.TotalCount,
            PageSize = paged.PageSize
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "CreateForm")] CreateEmployeeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Employees", "Search employees...");
            var invalid = await BuildIndexModelAsync(1, null, showCreate: true, createForm: model);
            return View("Index", invalid);
        }

        try
        {
            await _employeeService.CreateAsync(new CreateEmployeeDto
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                DepartmentId = model.DepartmentId,
                Salary = model.Salary,
                HireDate = model.HireDate,
                Role = model.Role,
                InitialPassword = model.InitialPassword
            });

            TempData["Success"] = "Employee created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("Employees", "Search employees...");
            var invalid = await BuildIndexModelAsync(1, null, showCreate: true, createForm: model);
            var fieldPrefix = ex.Message.Contains("email", StringComparison.OrdinalIgnoreCase)
                ? "CreateForm.Email"
                : "CreateForm.InitialPassword";
            ModalValidationHelper.AddFormErrors(ModelState, fieldPrefix, ex.Message);
            return View("Index", invalid);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind(Prefix = "EditForm")] EditEmployeeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Employees", "Search employees...");
            var invalid = await BuildIndexModelAsync(1, null, editId: model.Id);
            invalid.EditForm = model;
            return View("Index", invalid);
        }

        try
        {
            await _employeeService.UpdateAsync(new UpdateEmployeeDto
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                DepartmentId = model.DepartmentId,
                Salary = model.Salary,
                HireDate = model.HireDate,
                IsActive = model.IsActive
            });

            TempData["Success"] = "Employee updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("Employees", "Search employees...");
            var invalid = await BuildIndexModelAsync(1, null, editId: model.Id);
            invalid.EditForm = model;
            ModalValidationHelper.AddFormErrors(ModelState, "EditForm.Email", ex.Message);
            return View("Index", invalid);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _employeeService.SoftDeleteAsync(id);
        TempData["Success"] = "Employee deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        await _employeeService.RestoreAsync(id);
        TempData["Success"] = "Employee restored successfully.";
        return RedirectToAction(nameof(Deleted));
    }

    private async Task<EmployeeIndexViewModel> BuildIndexModelAsync(
        int page,
        int? departmentId,
        int? editId = null,
        bool showCreate = false,
        CreateEmployeeViewModel? createForm = null)
    {
        var departments = await HrDisplayHelper.LoadDepartmentsAsync(UnitOfWork);
        var paged = departmentId.HasValue
            ? await _employeeService.GetByDepartmentAsync(departmentId.Value, page, PageSize)
            : await _employeeService.GetAllAsync(page, PageSize);

        var canDeleteMap = new Dictionary<int, bool>();
        foreach (var employee in paged.Items)
            canDeleteMap[employee.Id] = await _employeeService.CanDeleteAsync(employee.Id);

        var model = new EmployeeIndexViewModel
        {
            Employees = paged.Items.Select(e => MapListItem(e, departments, canDeleteMap)).ToList(),
            Departments = ToDepartmentOptions(departments),
            CreateForm = createForm ?? new CreateEmployeeViewModel(),
            Page = paged.Page,
            TotalPages = paged.TotalPages,
            TotalCount = paged.TotalCount,
            PageSize = paged.PageSize,
            DepartmentFilter = departmentId,
            ShowCreateModal = showCreate
        };

        if (editId.HasValue)
        {
            var employee = await _employeeService.GetByIdAsync(editId.Value);
            model.EditForm = new EditEmployeeViewModel
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                DepartmentId = employee.DepartmentId,
                Salary = employee.Salary,
                HireDate = employee.HireDate,
                IsActive = employee.IsActive
            };
        }

        return model;
    }

    private static IReadOnlyList<DepartmentOptionViewModel> ToDepartmentOptions(IReadOnlyDictionary<int, Department> departments) =>
        departments.Values.Select(d => new DepartmentOptionViewModel { Id = d.Id, Name = d.Name }).OrderBy(d => d.Name).ToList();

    private static EmployeeListItemViewModel MapListItem(
        Data.Models.Employee employee,
        IReadOnlyDictionary<int, Data.Models.Department> departments,
        IReadOnlyDictionary<int, bool> canDeleteMap)
    {
        departments.TryGetValue(employee.DepartmentId, out var department);
        var isManager = department?.ManagerId == employee.Id;

        return new EmployeeListItemViewModel
        {
            Id = employee.Id,
            FullName = $"{employee.FirstName} {employee.LastName}",
            Email = employee.Email,
            DepartmentName = department?.Name ?? "—",
            IsManager = isManager,
            HireDate = employee.HireDate,
            CanDelete = canDeleteMap.GetValueOrDefault(employee.Id, !isManager),
            Initials = HrDisplayHelper.GetInitials(employee.FirstName, employee.LastName)
        };
    }
}
