using HRSystem.Common.Enums;

namespace HRSystem.Business.DTOs.Leave;

public class RequestLeaveDto
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public LeaveType LeaveType { get; set; }
    public string? Reason { get; set; }
}
