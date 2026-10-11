using HRSystem.Business.DTOs.Departments;
using HRSystem.Business.Exceptions;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using HRSystem.Web.Helpers;
using HRSystem.Web.ViewModels.HR;
using Microsoft.AspNetCore.Mvc;
using EmployeeEntity = HRSystem.Data.Models.Employee;

namespace HRSystem.Web.Areas.HR.Controllers;

public class DepartmentController : HRBaseController
{
    private const int PageSize = PaginationDefaults.DefaultPageSize;
    private readonly IDepartmentService _departmentService;

    public DepartmentController(
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IDepartmentService departmentService)
        : base(currentUser, unitOfWork)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, string? search = null, int? editId = null, int? replaceId = null, bool showCreate = false)
    {
        await SetLayoutAsync("Departments", "Search departments...");
        return View(await BuildIndexModelAsync(page, editId, replaceId, showCreate, search: search));
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        await SetLayoutAsync("Departments", "Search departments...");

        var department = await UnitOfWork.Departments.GetByIdAsync(id);
        if (department == null)
            return NotFound();

        var employeesPaged = await UnitOfWork.Employees.GetByDepartmentPagedAsync(id, 1, 1000);
        var employees = employeesPaged.Items;

        EmployeeEntity? manager = null;
        if (department.ManagerId > 0)
        {
            manager = await UnitOfWork.Employees.GetByIdAsync(department.ManagerId);
        }

        var activeEmployees = employees.Where(e => !e.IsDeleted && e.IsActive).ToList();
        var totalSalary = activeEmployees.Sum(e => e.Salary);
        var averageSalary = activeEmployees.Count > 0 ? Math.Round(totalSalary / activeEmployees.Count, 2) : 0m;

        var allEmployees = await HrDisplayHelper.LoadEmployeesAsync(UnitOfWork);
        var managerOptions = allEmployees.Values
            .Where(e => e is { IsActive: true, IsDeleted: false, IsHR: false })
            .Select(e => new ManagerOptionViewModel { Id = e.Id, Name = $"{e.FirstName} {e.LastName}" })
            .OrderBy(e => e.Name)
            .ToList();

        var model = new DepartmentDetailViewModel
        {
            Id = department.Id,
            Name = department.Name,
            IsDeleted = department.IsDeleted,
            ManagerId = department.ManagerId > 0 ? department.ManagerId : null,
            ManagerName = manager != null ? $"{manager.FirstName} {manager.LastName}" : "(Unassigned)",
            ManagerInitials = manager != null ? HrDisplayHelper.GetInitials(manager.FirstName, manager.LastName) : string.Empty,
            ManagerEmail = manager?.Email ?? string.Empty,
            ManagerPhone = manager?.PhoneNumber,
            ManagerAvatarUrl = manager?.ProfilePictureUrl,
            ManagerHireDate = manager?.HireDate,
            TotalEmployees = activeEmployees.Count,
            TotalSalaryBudget = totalSalary,
            AverageSalary = averageSalary,
            Employees = employees.Select(e => new DepartmentEmployeeItemViewModel
            {
                Id = e.Id,
                FullName = $"{e.FirstName} {e.LastName}",
                Initials = HrDisplayHelper.GetInitials(e.FirstName, e.LastName),
                AvatarUrl = e.ProfilePictureUrl,
                Email = e.Email ?? string.Empty,
                PhoneNumber = e.PhoneNumber,
                Salary = e.Salary,
                HireDate = e.HireDate,
                IsManager = e.Id == department.ManagerId,
                IsHR = e.IsHR,
                IsActive = e.IsActive && !e.IsDeleted
            }).OrderByDescending(e => e.IsManager).ThenBy(e => e.FullName).ToList(),
            ManagerOptions = managerOptions,
            EditForm = new EditDepartmentViewModel { Id = department.Id, Name = department.Name },
            ReplaceForm = new ReplaceManagerViewModel { DepartmentId = department.Id, DepartmentName = department.Name }
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Deleted(int page = 1, string? search = null)
    {
        await SetLayoutAsync("Departments", "Search deleted departments...");

        var employees = await HrDisplayHelper.LoadEmployeesAsync(UnitOfWork);
        var paged = await _departmentService.GetDeletedAsync(page, PageSize, search);

        return View("Index", new DepartmentIndexViewModel
        {
            Departments = MapDepartments(paged.Items, employees),
            Page = paged.Page,
            TotalPages = paged.TotalPages,
            TotalCount = paged.TotalCount,
            PageSize = paged.PageSize
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "CreateForm")] CreateDepartmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Departments", "Search departments...");
            var invalid = await BuildIndexModelAsync(1, showCreate: true);
            invalid.CreateForm = model;
            return View("Index", invalid);
        }

        try
        {
            await _departmentService.CreateAsync(new CreateDepartmentDto
            {
                Name = model.Name,
                ManagerId = model.ManagerId
            });

            TempData["Success"] = "Department created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (SoftDeletedNameConflictException conflict)
        {
            return await ConflictViewAsync(conflict, model.ManagerId, fromEdit: false);
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("Departments", "Search departments...");
            var invalid = await BuildIndexModelAsync(1, showCreate: true);
            invalid.CreateForm = model;
            ModelState.AddModelError("CreateForm.Name", ex.Message);
            return View("Index", invalid);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind(Prefix = "EditForm")] EditDepartmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Departments", "Search departments...");
            var invalid = await BuildIndexModelAsync(1, editId: model.Id);
            invalid.EditForm = model;
            return View("Index", invalid);
        }

        try
        {
            await _departmentService.UpdateAsync(new UpdateDepartmentDto
            {
                Id = model.Id,
                Name = model.Name
            });

            TempData["Success"] = "Department updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (SoftDeletedNameConflictException conflict)
        {
            return await ConflictViewAsync(conflict, managerId: null, fromEdit: true, editingDepartmentId: model.Id);
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("Departments", "Search departments...");
            var invalid = await BuildIndexModelAsync(1, editId: model.Id);
            invalid.EditForm = model;
            ModelState.AddModelError("EditForm.Name", ex.Message);
            return View("Index", invalid);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreDeleted(int id, int? managerId)
    {
        if (managerId is > 0)
        {
            var department = await _departmentService.RestoreWithManagerAsync(id, managerId.Value);
            TempData["Success"] = $"Department \"{department.Name}\" has been restored with its manager assigned.";
        }
        else
        {
            await _departmentService.RestoreAsync(id);
            TempData["Success"] = "Department restored successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReplaceManager([Bind(Prefix = "ReplaceForm")] ReplaceManagerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Departments", "Search departments...");
            var invalid = await BuildIndexModelAsync(1, replaceId: model.DepartmentId);
            invalid.ReplaceForm = model;
            return View("Index", invalid);
        }

        await _departmentService.ReplaceManagerAsync(model.DepartmentId, model.NewManagerId);
        TempData["Success"] = "Department manager updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _departmentService.SoftDeleteAsync(id);
        TempData["Success"] = "Department deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        await _departmentService.RestoreAsync(id);
        TempData["Success"] = "Department restored successfully.";
        return RedirectToAction(nameof(Deleted));
    }

    private async Task<DepartmentIndexViewModel> BuildIndexModelAsync(
        int page,
        int? editId = null,
        int? replaceId = null,
        bool showCreate = false,
        CreateDepartmentViewModel? createForm = null,
        string? search = null)
    {
        var employees = await HrDisplayHelper.LoadEmployeesAsync(UnitOfWork);
        var paged = await _departmentService.GetAllAsync(page, PageSize, search);

        var model = new DepartmentIndexViewModel
        {
            Departments = MapDepartments(paged.Items, employees),
            ManagerOptions = employees.Values
                .Where(e => e is { IsActive: true, IsDeleted: false, IsHR: false })
                .Select(e => new ManagerOptionViewModel { Id = e.Id, Name = $"{e.FirstName} {e.LastName}" })
                .OrderBy(e => e.Name)
                .ToList(),
            CreateForm = createForm ?? new CreateDepartmentViewModel(),
            Page = paged.Page,
            TotalPages = paged.TotalPages,
            TotalCount = paged.TotalCount,
            PageSize = paged.PageSize,
            ShowCreateModal = showCreate
        };

        if (editId.HasValue)
        {
            var department = await _departmentService.GetByIdAsync(editId.Value);
            model.EditForm = new EditDepartmentViewModel { Id = department.Id, Name = department.Name };
        }

        if (replaceId.HasValue)
        {
            var department = await _departmentService.GetByIdAsync(replaceId.Value);
            model.ReplaceForm = new ReplaceManagerViewModel
            {
                DepartmentId = department.Id,
                DepartmentName = department.Name
            };
        }

        return model;
    }

    private static IReadOnlyList<DepartmentListItemViewModel> MapDepartments(
        IEnumerable<Data.Models.Department> departments,
        IReadOnlyDictionary<int, Data.Models.Employee> employees)
    {
        var countsByDept = employees.Values
            .Where(e => !e.IsDeleted)
            .GroupBy(e => e.DepartmentId)
            .ToDictionary(g => g.Key, g => g.Count());

        var items = new List<DepartmentListItemViewModel>();
        foreach (var department in departments)
        {
            employees.TryGetValue(department.ManagerId, out var manager);
            var managerInDepartment = manager is { IsActive: true, IsDeleted: false }
                                      && manager.DepartmentId == department.Id;

            items.Add(new DepartmentListItemViewModel
            {
                Id = department.Id,
                Name = department.Name,
                HasManager = managerInDepartment,
                ManagerId = managerInDepartment ? manager!.Id : null,
                ManagerName = managerInDepartment ? $"{manager!.FirstName} {manager.LastName}" : "(Unassigned)",
                ManagerInitials = managerInDepartment ? HrDisplayHelper.GetInitials(manager!.FirstName, manager.LastName) : string.Empty,
                EmployeeCount = countsByDept.GetValueOrDefault(department.Id, 0)
            });
        }

        return items;
    }

    private async Task<IActionResult> ConflictViewAsync(
        SoftDeletedNameConflictException conflict,
        int? managerId,
        bool fromEdit,
        int? editingDepartmentId = null)
    {
        await SetLayoutAsync("Departments", "Search departments...");
        var model = await BuildIndexModelAsync(1, editId: fromEdit ? editingDepartmentId : null);
        model.DeletedNameConflict = new DeletedDepartmentNameConflictViewModel
        {
            DeletedDepartmentId = conflict.ResourceId,
            Name = conflict.Name,
            SelectedManagerId = managerId,
            FromEdit = fromEdit,
            EditingDepartmentId = editingDepartmentId
        };
        return View("Index", model);
    }
}
