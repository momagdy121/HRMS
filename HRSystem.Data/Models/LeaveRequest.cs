using System.ComponentModel.DataAnnotations;
using HRSystem.Common.Enums;

namespace HRSystem.Data.Models;

public class LeaveRequest
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public LeaveType LeaveType { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;

    public DateTime RequestDate { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }
}
