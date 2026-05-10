using System.ComponentModel.DataAnnotations;

namespace HRSystem.Data.Models;

public class Attendance
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateOnly Date { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    [MaxLength(200)]
    public string? Notes { get; set; }

    public bool IsDeleted { get; set; }
}
