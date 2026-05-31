using HRSystem.Business.DTOs;
using HRSystem.Business.DTOs.Tasks;
using HRSystem.Business.Exceptions;
using HRSystem.Business.Interfaces.Policies;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Business.Helpers;
using HRSystem.Business.Mapping;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using EmployeeTaskStatus = HRSystem.Common.Enums.TaskStatus;

namespace HRSystem.Business.Services;

public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ITaskAssignmentPolicy _assignmentPolicy;

    public TaskService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ITaskAssignmentPolicy assignmentPolicy)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _assignmentPolicy = assignmentPolicy;
    }

    public async Task<EmployeeTask> AssignAsync(AssignTaskDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsDepartmentHead())
            throw new BusinessRuleException("Only department heads can assign tasks.");

        var assigner = await _currentUser.GetCurrentEmployeeAsync(cancellationToken);
        var assignee = await _unitOfWork.Employees.GetByIdAsync(dto.AssignedToId, cancellationToken)
                       ?? throw new NotFoundException("Assignee not found.");

        if (!_assignmentPolicy.CanAssign(assigner, assignee))
        {
            throw new BusinessRuleException(
                "Tasks can only be assigned to non-HR employees in your department (not yourself).");
        }

        var task = EmployeeTaskMapper.FromDto(dto, assigner.Id, assignee.Id);

        await _unitOfWork.EmployeeTasks.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task UpdateStatusAsync(int taskId, EmployeeTaskStatus status, CancellationToken cancellationToken = default)
    {
        var task = await GetActiveTaskAsync(taskId, cancellationToken);
        var currentEmployee = await _currentUser.GetCurrentEmployeeAsync(cancellationToken);

        if (task.AssignedToId != currentEmployee.Id)
            throw new BusinessRuleException("Only the assigned employee can update task status.");

        if (!IsValidStatusTransition(task.Status, status))
        {
            throw new BusinessRuleException("Invalid status transition. Allowed: Pending → InProgress → Completed.");
        }

        TaskWorkflow.UpdateStatus(task, status);
        _unitOfWork.EmployeeTasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task EditTaskAsync(EditTaskDto dto, CancellationToken cancellationToken = default)
    {
        var task = await GetActiveTaskAsync(dto.Id, cancellationToken);
        var currentEmployee = await _currentUser.GetCurrentEmployeeAsync(cancellationToken);

        if (task.AssignedById != currentEmployee.Id)
            throw new BusinessRuleException("Only the task creator can edit task details.");

        if (task.Status == EmployeeTaskStatus.Completed)
            throw new BusinessRuleException("Completed tasks cannot be edited.");

        EmployeeTaskMapper.UpdateFromDto(task, dto);
        task.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.EmployeeTasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<EmployeeTask>> GetByManagerAsync(int managerEmployeeId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.EmployeeTasks.GetByAssignedByPagedAsync(managerEmployeeId, page, pageSize, cancellationToken);
        return PagedResultMapper.Map(result);
    }

    public async Task<PagedResult<EmployeeTask>> GetByEmployeeAsync(int employeeId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.EmployeeTasks.GetByAssignedToPagedAsync(employeeId, page, pageSize, cancellationToken);
        return PagedResultMapper.Map(result);
    }

    public async Task CancelTaskAsync(int taskId, CancellationToken cancellationToken = default)
    {
        var task = await GetActiveTaskAsync(taskId, cancellationToken);
        var currentEmployee = await _currentUser.GetCurrentEmployeeAsync(cancellationToken);

        if (task.AssignedById != currentEmployee.Id)
            throw new BusinessRuleException("Only the task creator can cancel a task.");

        TaskWorkflow.Cancel(task);
        _unitOfWork.EmployeeTasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<EmployeeTask> GetActiveTaskAsync(int taskId, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.EmployeeTasks.GetByIdAsync(taskId, cancellationToken)
                   ?? throw new NotFoundException("Task not found.");

        if (task.IsDeleted)
            throw new NotFoundException("Task not found.");

        return task;
    }

    private static bool IsValidStatusTransition(EmployeeTaskStatus current, EmployeeTaskStatus next) =>
        (current, next) switch
        {
            (EmployeeTaskStatus.Pending, EmployeeTaskStatus.InProgress) => true,
            (EmployeeTaskStatus.InProgress, EmployeeTaskStatus.Completed) => true,
            _ => false
        };
}
