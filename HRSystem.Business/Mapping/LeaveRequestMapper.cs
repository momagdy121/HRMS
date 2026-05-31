using HRSystem.Business.DTOs.Leave;
using HRSystem.Common.Enums;
using HRSystem.Data.Models;

namespace HRSystem.Business.Mapping;

public static class LeaveRequestMapper
{
    public static LeaveRequest FromDto(RequestLeaveDto dto, int employeeId) =>
        new()
        {
            EmployeeId = employeeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            LeaveType = dto.LeaveType,
            Reason = dto.Reason?.Trim(),
            Status = LeaveRequestStatus.Pending,
            RequestDate = DateTime.UtcNow
        };
}
