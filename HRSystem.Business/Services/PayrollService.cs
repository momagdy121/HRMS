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
    private readonly IWorkflowNotificationService _workflowNotifications;

    public PayrollService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IWorkflowNotificationService workflowNotifications)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _workflowNotifications = workflowNotifications;
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

    public async Task<BatchProcessPayrollResult> BatchProcessAsync(BatchProcessPayrollDto dto, CancellationToken cancellationToken = default)
    {
        EnsureHr();

        if (dto.Month is < 1 or > 12)
            throw new BusinessRuleException("Invalid month specified.");

        if (dto.Year < 2000 || dto.Year > DateTime.UtcNow.Year + 1)
            throw new BusinessRuleException("Invalid year specified.");

        var employees = await _unitOfWork.Employees.GetActiveForPayrollAsync(dto.DepartmentId, cancellationToken);
        var processor = await _currentUser.GetCurrentEmployeeAsync(cancellationToken);

        var result = new BatchProcessPayrollResult();
        var skippedDetails = new List<string>();

        foreach (var employee in employees)
        {
            var exists = await _unitOfWork.Payrolls.ExistsForEmployeeMonthYearAsync(employee.Id, dto.Month, dto.Year, cancellationToken);
            if (exists)
            {
                result.SkippedCount++;
                skippedDetails.Add($"{employee.FirstName} {employee.LastName} (already generated)");
                continue;
            }

            var baseSalary = employee.Salary > 0 ? employee.Salary : 0;
            var payroll = new Payroll
            {
                EmployeeId = employee.Id,
                Month = dto.Month,
                Year = dto.Year,
                BaseSalary = baseSalary,
                TotalBonus = 0,
                TotalDeduction = 0,
                NetSalary = baseSalary,
                Status = PayrollStatus.Draft,
                ProcessedBy = processor.Id,
                ProcessedAt = DateTime.UtcNow
            };

            await _unitOfWork.Payrolls.AddAsync(payroll, cancellationToken);
            result.CreatedCount++;
        }

        if (result.CreatedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        result.SkippedDetails = skippedDetails;
        return result;
    }

    public async Task<PayrollItem> AddPayrollItemAsync(AddPayrollItemDto dto, CancellationToken cancellationToken = default)
    {
        EnsureHr();

        var payroll = await GetDraftPayrollAsync(dto.PayrollId, cancellationToken);
        var item = PayrollItemMapper.FromDto(dto);

        await _unitOfWork.PayrollItems.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await RecalculatePayrollTotalsAsync(payroll, cancellationToken);
        return item;
    }

    public async Task UpdatePayrollItemAsync(EditPayrollItemDto dto, CancellationToken cancellationToken = default)
    {
        EnsureHr();

        var payroll = await GetDraftPayrollAsync(dto.PayrollId, cancellationToken);
        var item = await _unitOfWork.PayrollItems.GetByIdAsync(dto.Id, cancellationToken)
                   ?? throw new NotFoundException("Payroll item not found.", "Payroll", "Detail", "HR");

        if (item.PayrollId != payroll.Id)
            throw new BusinessRuleException("Payroll item does not belong to this payroll.");

        PayrollItemMapper.UpdateFromDto(item, dto);
        _unitOfWork.PayrollItems.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await RecalculatePayrollTotalsAsync(payroll, cancellationToken);
    }

    public async Task RemovePayrollItemAsync(int payrollItemId, CancellationToken cancellationToken = default)
    {
        EnsureHr();

        var item = await _unitOfWork.PayrollItems.GetByIdAsync(payrollItemId, cancellationToken)
                   ?? throw new NotFoundException("Payroll item not found.", "Payroll", "Detail", "HR");

        var payroll = await GetDraftPayrollAsync(item.PayrollId, cancellationToken);

        _unitOfWork.PayrollItems.Delete(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await RecalculatePayrollTotalsAsync(payroll, cancellationToken);
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

        var employee = await _unitOfWork.Employees.GetByIdAsync(payroll.EmployeeId, cancellationToken);
        if (employee != null)
        {
            if (status == PayrollStatus.Approved)
            {
                // In-app notification only — employee knows payroll is reviewed
                await _workflowNotifications.NotifyPayrollApprovedAsync(payroll, employee, cancellationToken);
            }
            else if (status == PayrollStatus.Paid)
            {
                // Full notification (in-app + email) — payslip is final
                await _workflowNotifications.NotifyPayslipAvailableAsync(payroll, employee, cancellationToken);
            }
        }
    }

    public async Task<BulkStatusUpdateResult> BulkApproveAsync(int? month = null, int? year = null, int? departmentId = null, CancellationToken cancellationToken = default)
    {
        EnsureHr();

        var drafts = await _unitOfWork.Payrolls.GetByStatusAndPeriodAsync(
            PayrollStatus.Draft, month, year, departmentId, cancellationToken);

        var result = new BulkStatusUpdateResult();

        foreach (var payroll in drafts)
        {
            if (payroll.BaseSalary <= 0)
            {
                result.SkippedCount++;
                continue;
            }

            payroll.Status = PayrollStatus.Approved;
            _unitOfWork.Payrolls.Update(payroll);
            result.UpdatedCount++;
        }

        if (result.UpdatedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send in-app notifications in bulk (no email at this stage)
            var approvedList = drafts.Where(p => p.Status == PayrollStatus.Approved).ToList();
            var employeeIds = approvedList.Select(p => p.EmployeeId).Distinct().ToList();
            var employeeMap = await _unitOfWork.Employees.GetByIdsAsync(employeeIds, cancellationToken);

            var pairs = new List<(Payroll payroll, Employee employee)>();
            foreach (var payroll in approvedList)
            {
                if (employeeMap.TryGetValue(payroll.EmployeeId, out var employee))
                {
                    pairs.Add((payroll, employee));
                }
            }

            await _workflowNotifications.NotifyBulkPayrollApprovedAsync(pairs, cancellationToken);
        }

        return result;
    }

    public async Task<BulkStatusUpdateResult> BulkMarkPaidAsync(int? month = null, int? year = null, int? departmentId = null, CancellationToken cancellationToken = default)
    {
        EnsureHr();

        var approved = await _unitOfWork.Payrolls.GetByStatusAndPeriodAsync(
            PayrollStatus.Approved, month, year, departmentId, cancellationToken);

        var result = new BulkStatusUpdateResult();

        foreach (var payroll in approved)
        {
            payroll.Status = PayrollStatus.Paid;
            _unitOfWork.Payrolls.Update(payroll);
            result.UpdatedCount++;
        }

        if (result.UpdatedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send notifications in bulk (batch in-app insertion + background email queue dispatch)
            var employeeIds = approved.Select(p => p.EmployeeId).Distinct().ToList();
            var employeeMap = await _unitOfWork.Employees.GetByIdsAsync(employeeIds, cancellationToken);

            var pairs = new List<(Payroll payroll, Employee employee)>();
            foreach (var payroll in approved)
            {
                if (employeeMap.TryGetValue(payroll.EmployeeId, out var employee))
                {
                    pairs.Add((payroll, employee));
                }
            }

            await _workflowNotifications.NotifyBulkPayslipsAvailableAsync(pairs, cancellationToken);
        }

        return result;
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

    public async Task<PagedResult<Payroll>> GetFilteredAsync(
        int? departmentId,
        int? month,
        int? year,
        PayrollStatus? status,
        int page = 1,
        int pageSize = 20,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Payrolls.GetFilteredPagedAsync(
            departmentId, month, year, status, page, pageSize, search, cancellationToken);
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

    private async Task<Payroll> GetDraftPayrollAsync(int payrollId, CancellationToken cancellationToken)
    {
        var payroll = await _unitOfWork.Payrolls.GetByIdAsync(payrollId, cancellationToken)
                      ?? throw new NotFoundException("Payroll not found.", "Payroll", "Index", "HR");

        if (payroll.Status != PayrollStatus.Draft)
            throw new BusinessRuleException("Payroll items can only be changed while status is Draft.");

        return payroll;
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
