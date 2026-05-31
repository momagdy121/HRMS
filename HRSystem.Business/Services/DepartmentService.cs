using HRSystem.Business.DTOs;
using HRSystem.Business.DTOs.Departments;
using HRSystem.Business.Exceptions;
using HRSystem.Business.Interfaces.Policies;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Business.Helpers;
using HRSystem.Business.Mapping;
using HRSystem.Common.Constants;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;

namespace HRSystem.Business.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountService _accountService;
    private readonly IDepartmentDeletionPolicy _deletionPolicy;
    private readonly IDepartmentManagerPolicy _managerPolicy;

    public DepartmentService(
        IUnitOfWork unitOfWork,
        IAccountService accountService,
        IDepartmentDeletionPolicy deletionPolicy,
        IDepartmentManagerPolicy managerPolicy)
    {
        _unitOfWork = unitOfWork;
        _accountService = accountService;
        _deletionPolicy = deletionPolicy;
        _managerPolicy = managerPolicy;
    }

    public async Task<PagedResult<Department>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Departments.GetActivePagedAsync(page, pageSize, cancellationToken);
        return PagedResultMapper.Map(result);
    }

    public async Task<PagedResult<Department>> GetDeletedAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Departments.GetDeletedPagedAsync(page, pageSize, cancellationToken);
        return PagedResultMapper.Map(result);
    }

    public async Task<Department> CreateAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        var manager = await _unitOfWork.Employees.GetByIdAsync(dto.ManagerId, cancellationToken)
                      ?? throw new NotFoundException("Manager employee not found.", "Department", "Index", "HR");

        if (manager.IsHR || !manager.IsActive || manager.IsDeleted)
        {
            throw new BusinessRuleException(
                "Manager must be a non-HR, active employee.");
        }

        var department = DepartmentMapper.FromDto(dto);

        await _unitOfWork.Departments.AddAsync(department, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (manager.DepartmentId != department.Id)
        {
            manager.DepartmentId = department.Id;
            _unitOfWork.Employees.Update(manager);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        await _accountService.ChangeRoleAsync(manager.Id, RoleNames.DepartmentHead, cancellationToken);

        return department;
    }

    public async Task UpdateAsync(UpdateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(dto.Id, cancellationToken)
                         ?? throw new NotFoundException("Department not found.", "Department", "Index", "HR");

        DepartmentMapper.UpdateFromDto(department, dto);
        _unitOfWork.Departments.Update(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(int departmentId, CancellationToken cancellationToken = default)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(departmentId, cancellationToken)
                         ?? throw new NotFoundException("Department not found.", "Department", "Index", "HR");

        if (!await _deletionPolicy.CanDeleteAsync(departmentId, cancellationToken))
        {
            var count = await _unitOfWork.Departments.CountActiveEmployeesAsync(departmentId, cancellationToken);
            throw new BusinessRuleException(
                $"Cannot delete department \"{department.Name}\": it still has {count} active employee(s). Reassign or remove them first.");
        }

        DepartmentLifecycle.MarkDeleted(department);
        _unitOfWork.Departments.Update(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreAsync(int departmentId, CancellationToken cancellationToken = default)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(departmentId, cancellationToken)
                         ?? throw new NotFoundException("Department not found.", "Department", "Deleted", "HR");

        if (!department.IsDeleted)
            throw new BusinessRuleException("Department is not deleted.");

        DepartmentLifecycle.MarkRestored(department);
        _unitOfWork.Departments.Update(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplaceManagerAsync(int departmentId, int newManagerId, CancellationToken cancellationToken = default)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(departmentId, cancellationToken)
                         ?? throw new NotFoundException("Department not found.", "Department", "Index", "HR");

        if (department.ManagerId == newManagerId)
            throw new BusinessRuleException("This employee is already the department manager.");

        var newManager = await _unitOfWork.Employees.GetByIdAsync(newManagerId, cancellationToken)
                         ?? throw new NotFoundException("New manager not found.", "Department", "Index", "HR");

        if (!_managerPolicy.IsValidManager(newManager, department))
        {
            throw new BusinessRuleException(
                "New manager must be a non-HR, active employee in this department.");
        }

        var oldManagerId = department.ManagerId;

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _accountService.ChangeRoleAsync(oldManagerId, RoleNames.Employee, cancellationToken);
            await _accountService.ChangeRoleAsync(newManagerId, RoleNames.DepartmentHead, cancellationToken);

            department.ManagerId = newManagerId;
            _unitOfWork.Departments.Update(department);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
