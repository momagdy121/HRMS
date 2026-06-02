using HRSystem.Business.DTOs.Departments;
using HRSystem.Business.Exceptions;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using HRSystem.Web.Helpers;
using HRSystem.Web.ViewModels.HR;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.HR.Controllers;

public class DepartmentController : HRBaseController
{
    private const int PageSize = 10;
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
    public async Task<IActionResult> Index(int page = 1, int? editId = null, int? replaceId = null, bool showCreate = false)
    {
        await SetLayoutAsync("Departments", "Search departments...");
        return View(await BuildIndexModelAsync(page, editId, replaceId, showCreate));
    }

    [HttpGet]
    public async Task<IActionResult> Deleted(int page = 1)
    {
        await SetLayoutAsync("Departments", "Search deleted departments...");

        var employees = await HrDisplayHelper.LoadEmployeesAsync(UnitOfWork);
        var paged = await _departmentService.GetDeletedAsync(page, PageSize);

        return View("Index", new DepartmentIndexViewModel
        {
            Departments = await MapDepartmentsAsync(paged.Items, employees),
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
        CreateDepartmentViewModel? createForm = null)
    {
        var employees = await HrDisplayHelper.LoadEmployeesAsync(UnitOfWork);
        var paged = await _departmentService.GetAllAsync(page, PageSize);

        var model = new DepartmentIndexViewModel
        {
            Departments = await MapDepartmentsAsync(paged.Items, employees),
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

    private async Task<IReadOnlyList<DepartmentListItemViewModel>> MapDepartmentsAsync(
        IEnumerable<Data.Models.Department> departments,
        IReadOnlyDictionary<int, Data.Models.Employee> employees)
    {
        var items = new List<DepartmentListItemViewModel>();
        foreach (var department in departments)
        {
            employees.TryGetValue(department.ManagerId, out var manager);
            var managerInDepartment = manager is { IsActive: true, IsDeleted: false }
                                      && manager.DepartmentId == department.Id;
            var count = await UnitOfWork.Departments.CountActiveEmployeesAsync(department.Id);

            items.Add(new DepartmentListItemViewModel
            {
                Id = department.Id,
                Name = department.Name,
                HasManager = managerInDepartment,
                ManagerName = managerInDepartment ? $"{manager!.FirstName} {manager.LastName}" : "(Unassigned)",
                ManagerInitials = managerInDepartment ? HrDisplayHelper.GetInitials(manager!.FirstName, manager.LastName) : string.Empty,
                EmployeeCount = count
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
