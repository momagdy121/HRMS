using HRSystem.Common.Enums;

namespace HRSystem.Data.Models;

public class LeaveBalance
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int Year { get; set; }

    public LeaveType LeaveType { get; set; }

    public int TotalDays { get; set; }

    public int UsedDays { get; set; }
}
