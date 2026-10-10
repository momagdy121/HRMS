using HRSystem.Common.Enums;
using HRSystem.Data.Models;

namespace HRSystem.Business.Helpers;

public static class LeaveBalanceDefaults
{
    public static int TotalDaysFor(LeaveType leaveType) =>
        leaveType switch
        {
            LeaveType.Annual => 21,
            LeaveType.Sick => 10,
            _ => throw new ArgumentOutOfRangeException(nameof(leaveType), leaveType, "Unpaid leave has no balance row.")
        };

    public static LeaveBalance Create(int employeeId, int year, LeaveType leaveType) =>
        new()
        {
            EmployeeId = employeeId,
            Year = year,
            LeaveType = leaveType,
            TotalDays = TotalDaysFor(leaveType),
            UsedDays = 0
        };
}
