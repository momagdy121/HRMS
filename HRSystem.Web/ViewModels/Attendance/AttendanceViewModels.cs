using System.ComponentModel.DataAnnotations;

namespace HRSystem.Web.ViewModels.Attendance;

public class AttendanceListItemViewModel
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public string DurationLabel { get; set; } = string.Empty;
}

public class EmployeeAttendanceIndexViewModel
{
    public bool CanCheckIn { get; set; }
    public bool CanCheckOut { get; set; }
    public bool ShowCheckInModal { get; set; }
    public bool ShowCheckOutModal { get; set; }

    public IReadOnlyList<AttendanceListItemViewModel> Records { get; set; } = [];

    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
}

public class TeamAttendanceRowViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeInitials { get; set; } = string.Empty;
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsSelf { get; set; }
}

public class MarkTeamAttendanceViewModel
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [Display(Name = "Check In")]
    public DateTime? CheckInTime { get; set; }

    [Display(Name = "Check Out")]
    public DateTime? CheckOutTime { get; set; }

    [StringLength(200)]
    public string? Notes { get; set; }

    public string EmployeeName { get; set; } = string.Empty;
}

public class DeptHeadAttendanceIndexViewModel
{
    public string DepartmentName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }

    public IReadOnlyList<TeamAttendanceRowViewModel> Team { get; set; } = [];

    public MarkTeamAttendanceViewModel? MarkForm { get; set; }
}

public class HrAttendanceRowViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeInitials { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class HrAttendanceIndexViewModel
{
    public DateOnly Date { get; set; }
    public int? DepartmentId { get; set; }

    public IReadOnlyList<(int Id, string Name)> Departments { get; set; } = [];
    public IReadOnlyList<HrAttendanceRowViewModel> Records { get; set; } = [];

    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
}

