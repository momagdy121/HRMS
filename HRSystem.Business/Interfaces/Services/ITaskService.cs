using HRSystem.Business.DTOs;
using HRSystem.Business.DTOs.Tasks;
using HRSystem.Data.Models;
using EmployeeTaskStatus = HRSystem.Common.Enums.TaskStatus;

namespace HRSystem.Business.Interfaces.Services;

public interface ITaskService
{
    Task<EmployeeTask> AssignAsync(AssignTaskDto dto, CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(UpdateTaskStatusDto dto, CancellationToken cancellationToken = default);

    Task EditTaskAsync(EditTaskDto dto, CancellationToken cancellationToken = default);

    Task<PagedResult<EmployeeTask>> GetByManagerAsync(int managerEmployeeId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    Task<PagedResult<EmployeeTask>> GetByEmployeeAsync(int employeeId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    Task CancelTaskAsync(int taskId, CancellationToken cancellationToken = default);
}
