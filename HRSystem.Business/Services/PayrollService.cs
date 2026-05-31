using HRSystem.Business.DTOs;
using HRSystem.Business.DTOs.Payroll;
using HRSystem.Business.Exceptions;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Business.Helpers;
using HRSystem.Business.Mapping;
using HRSystem.Common.Enums;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;

namespace HRSystem.Business.Services;

public class PayrollService : IPayrollService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public PayrollService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Payroll> ProcessAsync(ProcessPayrollDto dto, CancellationToken cancellationToken = default)
    {
        EnsureHr();

        var employee = await _unitOfWork.Employees.GetByIdAsync(dto.EmployeeId, cancellationToken)
                       ?? throw new NotFoundException("Employee not found.", "Payroll", "Index", "HR");

        if (await _unitOfWork.Payrolls.ExistsForEmployeeMonthYearAsync(dto.EmployeeId, dto.Month, dto.Year, cancellationToken))
        {
            throw new BusinessRuleException("Payroll already exists for this employee in the selected month.");
        }

        var processor = await _currentUser.GetCurrentEmployeeAsync(cancellationToken);
        var baseSalary = dto.BaseSalary > 0 ? dto.BaseSalary : employee.Salary;
        var payroll = PayrollMapper.FromDto(dto, processor.Id, baseSalary);

        await _unitOfWork.Payrolls.AddAsync(payroll, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return payroll;
    }

    public async Task<PayrollItem> AddPayrollItemAsync(AddPayrollItemDto dto, CancellationToken cancellationToken = default)
    {
        EnsureHr();

        var payroll = await _unitOfWork.Payrolls.GetByIdAsync(dto.PayrollId, cancellationToken)
                      ?? throw new NotFoundException("Payroll not found.", "Payroll", "Index", "HR");

        if (payroll.Status != PayrollStatus.Draft)
            throw new BusinessRuleException("Payroll items can only be added while status is Draft.");

        var item = PayrollItemMapper.FromDto(dto);

        await _unitOfWork.PayrollItems.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await RecalculatePayrollTotalsAsync(payroll, cancellationToken);
        return item;
    }

    public async Task UpdateStatusAsync(int payrollId, PayrollStatus status, CancellationToken cancellationToken = default)
    {
        EnsureHr();

        var payroll = await _unitOfWork.Payrolls.GetByIdAsync(payrollId, cancellationToken)
                      ?? throw new NotFoundException("Payroll not found.", "Payroll", "Index", "HR");

        if (!IsValidStatusTransition(payroll.Status, status))
        {
            throw new BusinessRuleException("Invalid payroll status transition. Allowed: Draft → Approved → Paid.");
        }

        payroll.Status = status;
        _unitOfWork.Payrolls.Update(payroll);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<Payroll>> GetByEmployeeAsync(int employeeId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Payrolls.GetByEmployeePagedAsync(employeeId, page, pageSize, cancellationToken);
        return PagedResultMapper.Map(result);
    }

    public async Task<PagedResult<Payroll>> GetByDepartmentAsync(int departmentId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Payrolls.GetByDepartmentPagedAsync(departmentId, page, pageSize, cancellationToken);
        return PagedResultMapper.Map(result);
    }

    public async Task<PagedResult<Payroll>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Payrolls.GetAllPagedAsync(page, pageSize, cancellationToken);
        return PagedResultMapper.Map(result);
    }

    private async Task RecalculatePayrollTotalsAsync(Payroll payroll, CancellationToken cancellationToken)
    {
        var bonus = await _unitOfWork.PayrollItems.GetBonusTotalAsync(payroll.Id, cancellationToken);
        var deduction = await _unitOfWork.PayrollItems.GetDeductionTotalAsync(payroll.Id, cancellationToken);

        PayrollCalculator.ApplyTotals(payroll, bonus, deduction);
        _unitOfWork.Payrolls.Update(payroll);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private void EnsureHr()
    {
        if (!_currentUser.IsHR())
            throw new BusinessRuleException("Only HR can process payroll.");
    }

    private static bool IsValidStatusTransition(PayrollStatus current, PayrollStatus next) =>
        (current, next) switch
        {
            (PayrollStatus.Draft, PayrollStatus.Approved) => true,
            (PayrollStatus.Approved, PayrollStatus.Paid) => true,
            _ => false
        };
}
