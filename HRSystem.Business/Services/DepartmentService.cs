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
using Microsoft.AspNetCore.Identity;

namespace HRSystem.Business.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDepartmentDeletionPolicy _deletionPolicy;
    private readonly IDepartmentManagerPolicy _managerPolicy;

    public DepartmentService(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        IDepartmentDeletionPolicy deletionPolicy,
        IDepartmentManagerPolicy managerPolicy)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
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

    public async Task<Department> GetByIdAsync(int departmentId, CancellationToken cancellationToken = default) =>
        await _unitOfWork.Departments.GetByIdAsync(departmentId, cancellationToken)
        ?? throw new NotFoundException("Department not found.", "Department", "Index", "HR");

    public async Task<Department> CreateAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        await EnsureNameAvailableForCreateAsync(dto.Name, cancellationToken);

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

        await SyncManagerRoleAsync(manager.Id, cancellationToken);

        return department;
    }

    public async Task UpdateAsync(UpdateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(dto.Id, cancellationToken)
                         ?? throw new NotFoundException("Department not found.", "Department", "Index", "HR");

        await EnsureNameAvailableForUpdateAsync(dto.Name, dto.Id, cancellationToken);

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

    public async Task<Department> RestoreWithManagerAsync(int departmentId, int managerId, CancellationToken cancellationToken = default)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(departmentId, cancellationToken)
                         ?? throw new NotFoundException("Department not found.", "Department", "Index", "HR");

        if (!department.IsDeleted)
            throw new BusinessRuleException("Department is not deleted.");

        var manager = await _unitOfWork.Employees.GetByIdAsync(managerId, cancellationToken)
                      ?? throw new NotFoundException("Manager employee not found.", "Department", "Index", "HR");

        if (manager.IsHR || !manager.IsActive || manager.IsDeleted)
        {
            throw new BusinessRuleException("Manager must be a non-HR, active employee.");
        }

        DepartmentLifecycle.MarkRestored(department);
        department.ManagerId = managerId;
        _unitOfWork.Departments.Update(department);

        if (manager.DepartmentId != department.Id)
        {
            manager.DepartmentId = department.Id;
            _unitOfWork.Employees.Update(manager);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await SyncManagerRoleAsync(manager.Id, cancellationToken);

        return department;
    }

    public async Task ReplaceManagerAsync(int departmentId, int newManagerId, CancellationToken cancellationToken = default)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(departmentId, cancellationToken)
                         ?? throw new NotFoundException("Department not found.", "Department", "Index", "HR");

        if (department.ManagerId == newManagerId)
            throw new BusinessRuleException("This employee is already the department manager.");

        var newManager = await _unitOfWork.Employees.GetByIdAsync(newManagerId, cancellationToken)
                         ?? throw new NotFoundException("New manager not found.", "Department", "Index", "HR");

        if (newManager.IsHR || !newManager.IsActive || newManager.IsDeleted)
        {
            throw new BusinessRuleException(
                "New manager must be a non-HR, active employee.");
        }

        var oldManagerId = department.ManagerId;
        var oldManager = await _unitOfWork.Employees.GetByIdAsync(oldManagerId, cancellationToken);

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            if (newManager.DepartmentId != department.Id)
            {
                newManager.DepartmentId = department.Id;
                _unitOfWork.Employees.Update(newManager);
            }

            if (oldManagerId != newManagerId && oldManager?.DepartmentId == departmentId)
            {
                await SyncEmployeeRoleAsync(oldManagerId, RoleNames.Employee, cancellationToken);
            }

            await SyncManagerRoleAsync(newManagerId, cancellationToken);

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

    private async Task EnsureNameAvailableForCreateAsync(string name, CancellationToken cancellationToken)
    {
        var normalized = name.Trim();
        var existing = await _unitOfWork.Departments.GetByNameAsync(normalized, cancellationToken);
        if (existing == null)
            return;

        if (existing.IsDeleted)
        {
            throw new SoftDeletedNameConflictException("Department", existing.Id, existing.Name);
        }

        throw new BusinessRuleException("A department with this name already exists.");
    }

    private async Task EnsureNameAvailableForUpdateAsync(string name, int departmentId, CancellationToken cancellationToken)
    {
        var normalized = name.Trim();
        var existing = await _unitOfWork.Departments.GetByNameAsync(normalized, cancellationToken);
        if (existing == null || existing.Id == departmentId)
            return;

        if (existing.IsDeleted)
        {
            throw new SoftDeletedNameConflictException("Department", existing.Id, existing.Name);
        }

        throw new BusinessRuleException("A department with this name already exists.");
    }

    private Task SyncManagerRoleAsync(int employeeId, CancellationToken cancellationToken) =>
        SyncEmployeeRoleAsync(employeeId, RoleNames.DepartmentHead, cancellationToken);

    private async Task SyncEmployeeRoleAsync(int employeeId, string role, CancellationToken cancellationToken)
    {
        if (!RoleNames.AllRoles.Contains(role))
            throw new BusinessRuleException($"Invalid role '{role}'.");

        var user = await _unitOfWork.ApplicationUsers.GetByEmployeeIdAsync(employeeId, cancellationToken)
                   ?? throw new NotFoundException("User account not found for this employee.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                throw new BusinessRuleException(
                    string.Join("; ", removeResult.Errors.Select(e => e.Description)));
            }
        }

        var addResult = await _userManager.AddToRoleAsync(user, role);
        if (!addResult.Succeeded)
        {
            throw new BusinessRuleException(
                string.Join("; ", addResult.Errors.Select(e => e.Description)));
        }
    }
}
