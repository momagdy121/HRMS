using HRSystem.Business.DTOs;
using HRSystem.Business.DTOs.Attendance;
using HRSystem.Business.Exceptions;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Business.Helpers;
using HRSystem.Business.Mapping;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;

namespace HRSystem.Business.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public AttendanceService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Attendance> CheckInAsync(CancellationToken cancellationToken = default)
    {
        var employee = await _currentUser.GetCurrentEmployeeAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var checkInTime = DateTime.UtcNow;

        var existing = await _unitOfWork.Attendances.GetByEmployeeAndDateAsync(employee.Id, today, cancellationToken);
        if (existing?.CheckInTime != null)
            throw new BusinessRuleException("You have already checked in today.");

        if (existing != null)
        {
            AttendanceWorkflow.ApplyCheckIn(existing, checkInTime);
            _unitOfWork.Attendances.Update(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return existing;
        }

        var attendance = AttendanceWorkflow.CreateForCheckIn(employee.Id, today, checkInTime);

        await _unitOfWork.Attendances.AddAsync(attendance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return attendance;
    }

    public async Task<Attendance> CheckOutAsync(CancellationToken cancellationToken = default)
    {
        var employee = await _currentUser.GetCurrentEmployeeAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var attendance = await _unitOfWork.Attendances.GetByEmployeeAndDateAsync(employee.Id, today, cancellationToken)
                         ?? throw new BusinessRuleException("You must check in before checking out.");

        if (attendance.CheckInTime == null)
            throw new BusinessRuleException("You must check in before checking out.");

        if (attendance.CheckOutTime != null)
            throw new BusinessRuleException("You have already checked out today.");

        var checkOutTime = DateTime.UtcNow;
        if (checkOutTime <= attendance.CheckInTime)
            throw new BusinessRuleException("Check-out time must be after check-in time.");

        AttendanceWorkflow.ApplyCheckOut(attendance, checkOutTime);
        _unitOfWork.Attendances.Update(attendance);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return attendance;
    }

    public async Task<Attendance> MarkTeamAttendanceAsync(MarkTeamAttendanceDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsDepartmentHead())
            throw new BusinessRuleException("Only department heads can mark team attendance.");

        var manager = await _currentUser.GetCurrentEmployeeAsync(cancellationToken);
        var employee = await _unitOfWork.Employees.GetByIdAsync(dto.EmployeeId, cancellationToken)
                       ?? throw new NotFoundException("Employee not found.");

        if (employee.DepartmentId != manager.DepartmentId)
            throw new BusinessRuleException("You can only mark attendance for employees in your department.");

        if (dto.CheckOutTime.HasValue && dto.CheckInTime.HasValue && dto.CheckOutTime <= dto.CheckInTime)
            throw new BusinessRuleException("Check-out time must be after check-in time.");

        var existing = await _unitOfWork.Attendances.GetByEmployeeAndDateAsync(dto.EmployeeId, dto.Date, cancellationToken);
        if (existing != null)
        {
            AttendanceMapper.UpdateFromDto(existing, dto);
            _unitOfWork.Attendances.Update(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return existing;
        }

        var attendance = AttendanceMapper.FromDto(dto);

        await _unitOfWork.Attendances.AddAsync(attendance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return attendance;
    }

    public async Task<PagedResult<Attendance>> GetByEmployeeAsync(int employeeId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Attendances.GetByEmployeePagedAsync(employeeId, page, pageSize, cancellationToken);
        return PagedResultMapper.Map(result);
    }

    public async Task<PagedResult<Attendance>> GetByDepartmentAsync(int departmentId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Attendances.GetByDepartmentPagedAsync(departmentId, page, pageSize, cancellationToken);
        return PagedResultMapper.Map(result);
    }
}
