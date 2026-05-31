namespace HRSystem.Business.DTOs.Attendance;

public class MarkTeamAttendanceDto
{
    public int EmployeeId { get; set; }
    public DateOnly Date { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string? Notes { get; set; }
}
