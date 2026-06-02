using HRSystem.Business.DTOs.Tasks;
using HRSystem.Business.Exceptions;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using EmployeeEntity = HRSystem.Data.Models.Employee;
using HRSystem.Web.Helpers;
using HRSystem.Web.ViewModels.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskStatus = HRSystem.Common.Enums.TaskStatus;

namespace HRSystem.Web.Areas.DepartmentHead.Controllers;

public class TaskController : DepartmentHeadBaseController
{
    private const int PageSize = 10;
    private const int AssigneePageSize = 100;
    private readonly ITaskService _taskService;

    public TaskController(
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        ITaskService taskService)
        : base(currentUser, unitOfWork)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, bool showAssign = false, int? editId = null, int? cancelId = null)
    {
        await SetLayoutAsync("Tasks", "Search tasks...");
        return View(await BuildIndexModelAsync(page, showAssign, editId, cancelId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign([Bind(Prefix = "AssignForm")] AssignTaskViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Tasks", "Search tasks...");
            return View("Index", await BuildIndexModelAsync(1, showAssign: true, assignForm: model));
        }

        try
        {
            await _taskService.AssignAsync(new AssignTaskDto
            {
                Title = model.Title,
                Description = model.Description,
                AssignedToId = model.AssignedToId,
                DueDate = model.DueDate
            });

            TempData["Success"] = "Task assigned successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("Tasks", "Search tasks...");
            ModalValidationHelper.AddFormErrors(ModelState, "AssignForm", ex.Message);
            return View("Index", await BuildIndexModelAsync(1, showAssign: true, assignForm: model));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind(Prefix = "EditForm")] EditTaskViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Tasks", "Search tasks...");
            var invalid = await BuildIndexModelAsync(1, editId: model.Id);
            invalid.EditForm = model;
            return View("Index", invalid);
        }

        try
        {
            await _taskService.EditTaskAsync(new EditTaskDto
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                DueDate = model.DueDate
            });

            TempData["Success"] = "Task updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("Tasks", "Search tasks...");
            var invalid = await BuildIndexModelAsync(1, editId: model.Id);
            invalid.EditForm = model;
            ModalValidationHelper.AddFormErrors(ModelState, "EditForm", ex.Message);
            return View("Index", invalid);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _taskService.CancelTaskAsync(id);
            TempData["Success"] = "Task cancelled successfully.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<DeptHeadTaskIndexViewModel> BuildIndexModelAsync(
        int page,
        bool showAssign = false,
        int? editId = null,
        int? cancelId = null,
        AssignTaskViewModel? assignForm = null)
    {
        var manager = await CurrentUser.GetCurrentEmployeeAsync();
        var paged = await _taskService.GetByManagerAsync(manager.Id, page, PageSize);
        var employees = await LoadEmployeesForTasksAsync(paged.Items);
        var assignees = await LoadAssignableEmployeesAsync(manager);

        var tasks = paged.Items
            .Select(task => MapListItem(task, employees, manager.Id))
            .ToList();

        EditTaskViewModel? editForm = null;
        if (editId.HasValue)
        {
            var task = await FindManagerTaskAsync(editId.Value, manager.Id, paged.Items);
            if (task != null && task.Status != TaskStatus.Completed)
            {
                editForm = new EditTaskViewModel
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    DueDate = task.DueDate
                };
            }
        }

        CancelTaskViewModel? cancelTarget = null;
        if (cancelId.HasValue)
        {
            var task = await FindManagerTaskAsync(cancelId.Value, manager.Id, paged.Items);
            if (task != null && task.Status != TaskStatus.Completed)
            {
                employees.TryGetValue(task.AssignedToId, out var assignee);
                cancelTarget = new CancelTaskViewModel
                {
                    Id = task.Id,
                    Title = task.Title,
                    AssigneeName = assignee != null ? TaskDisplayHelper.GetFullName(assignee) : "Unknown"
                };
            }
        }

        return new DeptHeadTaskIndexViewModel
        {
            Tasks = tasks,
            Assignees = assignees,
            AssignForm = assignForm ?? new AssignTaskViewModel(),
            EditForm = editForm,
            CancelTarget = cancelTarget,
            ShowAssignModal = showAssign,
            Page = paged.Page,
            TotalPages = paged.TotalPages,
            TotalCount = paged.TotalCount,
            PageSize = paged.PageSize
        };
    }

    private static TaskListItemViewModel MapListItem(
        EmployeeTask task,
        IReadOnlyDictionary<int, EmployeeEntity> employees,
        int managerId)
    {
        employees.TryGetValue(task.AssignedToId, out var assignee);
        var canModify = task.AssignedById == managerId && task.Status != TaskStatus.Completed;

        return new TaskListItemViewModel
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            PersonName = assignee != null ? TaskDisplayHelper.GetFullName(assignee) : "Unknown",
            PersonInitials = assignee != null ? TaskDisplayHelper.GetInitials(assignee) : "?",
            DueDate = task.DueDate,
            IsOverdue = TaskDisplayHelper.IsOverdue(task),
            Status = task.Status,
            CompletionNotes = task.CompletionNotes,
            CanEdit = canModify,
            CanCancel = canModify
        };
    }

    private async Task<EmployeeTask?> FindManagerTaskAsync(
        int taskId,
        int managerId,
        IReadOnlyList<EmployeeTask> pageTasks)
    {
        var task = pageTasks.FirstOrDefault(t => t.Id == taskId)
                   ?? await UnitOfWork.EmployeeTasks.GetByIdAsync(taskId);

        if (task == null || task.IsDeleted || task.AssignedById != managerId)
            return null;

        return task;
    }

    private async Task<IReadOnlyList<TaskEmployeeOptionViewModel>> LoadAssignableEmployeesAsync(EmployeeEntity manager)
    {
        var department = await UnitOfWork.Departments.GetByManagerIdAsync(manager.Id);
        if (department == null)
            return [];

        var paged = await UnitOfWork.Employees.GetByDepartmentPagedAsync(
            department.Id,
            1,
            AssigneePageSize);

        return paged.Items
            .Where(e => e.IsActive && !e.IsDeleted && !e.IsHR && e.Id != manager.Id)
            .Select(e => new TaskEmployeeOptionViewModel
            {
                Id = e.Id,
                Name = TaskDisplayHelper.GetFullName(e)
            })
            .ToList();
    }

    private async Task<Dictionary<int, EmployeeEntity>> LoadEmployeesForTasksAsync(IReadOnlyList<EmployeeTask> tasks)
    {
        var ids = tasks
            .SelectMany(t => new[] { t.AssignedById, t.AssignedToId })
            .Distinct();

        var employees = new Dictionary<int, EmployeeEntity>();
        foreach (var id in ids)
        {
            var employee = await UnitOfWork.Employees.GetByIdAsync(id);
            if (employee != null)
                employees[id] = employee;
        }

        return employees;
    }
}
