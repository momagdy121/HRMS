namespace HRSystem.Business.DTOs.Attendance;

public class MonthlyTimesheetDto
{
    public int EmployeeId { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public int DaysPresent { get; set; }

    public int WeekdayAbsences { get; set; }

    public int LateCheckIns { get; set; }

    public double TotalHours { get; set; }

    public IReadOnlyList<MonthlyTimesheetDayDto> Days { get; set; } = [];
}

public class MonthlyTimesheetDayDto
{
    public DateOnly Date { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public bool IsPresent { get; set; }

    public bool IsLate { get; set; }

    public bool IsWeekday { get; set; }

    public bool IsFuture { get; set; }

    public double? HoursWorked { get; set; }
}
