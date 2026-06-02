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

namespace HRSystem.Web.Areas.Employee.Controllers;

public class TaskController : EmployeeBaseController
{
    private const int PageSize = 10;
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
    public async Task<IActionResult> Index(int page = 1, int? updateId = null)
    {
        await SetLayoutAsync("Tasks", "Search tasks...");
        return View(await BuildIndexModelAsync(page, updateId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus([Bind(Prefix = "UpdateStatusForm")] UpdateTaskStatusViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Tasks", "Search tasks...");
            return View("Index", await BuildIndexModelAsync(1, updateId: model.Id, updateForm: model));
        }

        try
        {
            await _taskService.UpdateStatusAsync(new UpdateTaskStatusDto
            {
                TaskId = model.Id,
                Status = model.Status,
                CompletionNotes = model.CompletionNotes
            });
            TempData["Success"] = "Task status updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("Tasks", "Search tasks...");
            ModalValidationHelper.AddFormErrors(ModelState, "UpdateStatusForm", ex.Message);
            return View("Index", await BuildIndexModelAsync(1, updateId: model.Id, updateForm: model));
        }
    }

    private async Task<EmployeeTaskIndexViewModel> BuildIndexModelAsync(
        int page,
        int? updateId = null,
        UpdateTaskStatusViewModel? updateForm = null)
    {
        var employee = await CurrentUser.GetCurrentEmployeeAsync();
        var paged = await _taskService.GetByEmployeeAsync(employee.Id, page, PageSize);
        var employees = await LoadEmployeesForTasksAsync(paged.Items);

        var tasks = paged.Items
            .Select(task => MapListItem(task, employees))
            .ToList();

        UpdateTaskStatusViewModel? resolvedUpdateForm = null;
        if (updateId.HasValue || updateForm != null)
        {
            var taskId = updateForm?.Id ?? updateId!.Value;
            var task = paged.Items.FirstOrDefault(t => t.Id == taskId)
                       ?? await UnitOfWork.EmployeeTasks.GetByIdAsync(taskId);

            if (task != null
                && !task.IsDeleted
                && task.AssignedToId == employee.Id
                && task.Status != TaskStatus.Completed)
            {
                resolvedUpdateForm = BuildUpdateForm(task);
                if (updateForm != null)
                {
                    resolvedUpdateForm.Status = updateForm.Status;
                    resolvedUpdateForm.CompletionNotes = updateForm.CompletionNotes;
                }
            }
        }

        return new EmployeeTaskIndexViewModel
        {
            Tasks = tasks,
            UpdateStatusForm = resolvedUpdateForm,
            Page = paged.Page,
            TotalPages = paged.TotalPages,
            TotalCount = paged.TotalCount,
            PageSize = paged.PageSize
        };
    }

    private static TaskListItemViewModel MapListItem(
        EmployeeTask task,
        IReadOnlyDictionary<int, EmployeeEntity> employees)
    {
        employees.TryGetValue(task.AssignedById, out var assigner);
        var allowedNext = TaskDisplayHelper.GetAllowedNextStatuses(task.Status);

        return new TaskListItemViewModel
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            PersonName = assigner != null ? TaskDisplayHelper.GetFullName(assigner) : "Unknown",
            PersonInitials = assigner != null ? TaskDisplayHelper.GetInitials(assigner) : "?",
            DueDate = task.DueDate,
            IsOverdue = TaskDisplayHelper.IsOverdue(task),
            Status = task.Status,
            CompletionNotes = task.CompletionNotes,
            CanUpdateStatus = allowedNext.Count > 0
        };
    }

    private static UpdateTaskStatusViewModel BuildUpdateForm(EmployeeTask task)
    {
        var allowed = TaskDisplayHelper.GetAllowedNextStatuses(task.Status);
        return new UpdateTaskStatusViewModel
        {
            Id = task.Id,
            TaskTitle = task.Title,
            CurrentStatus = task.Status,
            Status = allowed[0],
            AllowedNextStatuses = allowed
        };
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
