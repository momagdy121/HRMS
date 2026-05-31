using HRSystem.Business.DTOs;
using HRSystem.Business.DTOs.Employees;
using HRSystem.Business.Exceptions;
using HRSystem.Business.Interfaces.Policies;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Business.Helpers;
using HRSystem.Business.Mapping;
using HRSystem.Common.Constants;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;

namespace HRSystem.Business.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountService _accountService;
    private readonly IEmployeeDeletionPolicy _deletionPolicy;
    private readonly IDepartmentManagerPolicy _managerPolicy;

    public EmployeeService(
        IUnitOfWork unitOfWork,
        IAccountService accountService,
        IEmployeeDeletionPolicy deletionPolicy,
        IDepartmentManagerPolicy managerPolicy)
    {
        _unitOfWork = unitOfWork;
        _accountService = accountService;
        _deletionPolicy = deletionPolicy;
        _managerPolicy = managerPolicy;
    }

    public async Task<PagedResult<Employee>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Employees.GetActivePagedAsync(page, pageSize, cancellationToken);
        return PagedResultMapper.Map(result);
    }

    public async Task<PagedResult<Employee>> GetByDepartmentAsync(int departmentId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Employees.GetByDepartmentPagedAsync(departmentId, page, pageSize, cancellationToken);
        return PagedResultMapper.Map(result);
    }

    public async Task<PagedResult<Employee>> GetDeletedAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Employees.GetDeletedPagedAsync(page, pageSize, cancellationToken);
        return PagedResultMapper.Map(result);
    }

    public async Task<Employee> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        ValidateRole(dto.Role);

        if (await _unitOfWork.Employees.EmailExistsAsync(dto.Email, cancellationToken: cancellationToken))
            throw new BusinessRuleException("An employee with this email already exists.");

        var department = await _unitOfWork.Departments.GetByIdAsync(dto.DepartmentId, cancellationToken)
                         ?? throw new NotFoundException("Department not found.", "Department", "Index", "HR");

        var candidate = EmployeeLifecycle.CreateManagerValidationCandidate(dto);

        if (dto.Role == RoleNames.DepartmentHead && !_managerPolicy.IsValidManager(candidate, department))
        {
            throw new BusinessRuleException(
                "Department head must be a non-HR, active employee in the same department.");
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var employee = EmployeeMapper.FromDto(dto);

            await _unitOfWork.Employees.AddAsync(employee, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _accountService.CreateAccountAsync(employee.Id, employee.Email, dto.InitialPassword, dto.Role, cancellationToken);

            if (dto.Role == RoleNames.DepartmentHead)
            {
                department.ManagerId = employee.Id;
                _unitOfWork.Departments.Update(department);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return employee;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateAsync(UpdateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(dto.Id, cancellationToken)
                       ?? throw new NotFoundException("Employee not found.", "Employee", "Index", "HR");

        if (await _unitOfWork.Employees.EmailExistsAsync(dto.Email, dto.Id, cancellationToken))
            throw new BusinessRuleException("An employee with this email already exists.");

        EmployeeMapper.UpdateFromDto(employee, dto);

        _unitOfWork.Employees.Update(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        if (!await CanDeleteAsync(employeeId, cancellationToken))
        {
            throw new BusinessRuleException(
                "This employee is a department manager. Assign a replacement manager before deleting.");
        }

        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId, cancellationToken)
                       ?? throw new NotFoundException("Employee not found.", "Employee", "Index", "HR");

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            EmployeeLifecycle.MarkDeleted(employee);
            _unitOfWork.Employees.Update(employee);

            await _unitOfWork.EmployeeTasks.SoftDeleteAllForEmployeeAsync(employeeId, cancellationToken);
            await _unitOfWork.Attendances.SoftDeleteAllForEmployeeAsync(employeeId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task RestoreAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId, cancellationToken)
                       ?? throw new NotFoundException("Employee not found.", "Employee", "Deleted", "HR");

        if (!employee.IsDeleted)
            throw new BusinessRuleException("Employee is not deleted.");

        EmployeeLifecycle.MarkRestored(employee);
        _unitOfWork.Employees.Update(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> CanDeleteAsync(int employeeId, CancellationToken cancellationToken = default) =>
        _deletionPolicy.CanDeleteAsync(employeeId, cancellationToken);

    private static void ValidateRole(string role)
    {
        if (!RoleNames.AllRoles.Contains(role))
            throw new BusinessRuleException($"Invalid role '{role}'.");
    }
}
