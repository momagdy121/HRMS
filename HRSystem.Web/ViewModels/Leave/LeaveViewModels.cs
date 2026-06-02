using System.ComponentModel.DataAnnotations;
using HRSystem.Common.Enums;

namespace HRSystem.Web.ViewModels.Leave;

public class LeaveListItemViewModel
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeInitials { get; set; } = string.Empty;
    public LeaveType LeaveType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int Days { get; set; }
    public string? Reason { get; set; }
    public LeaveRequestStatus Status { get; set; }
    public bool CanAction { get; set; }
}

public class LeaveBalanceCardViewModel
{
    public LeaveType LeaveType { get; set; }
    public int TotalDays { get; set; }
    public int UsedDays { get; set; }
    public int RemainingDays => TotalDays - UsedDays;
}

public class RequestLeaveViewModel
{
    [Required]
    [Display(Name = "Leave Type")]
    public LeaveType LeaveType { get; set; } = LeaveType.Annual;

    [Required]
    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    [Display(Name = "End Date")]
    [DataType(DataType.Date)]
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [StringLength(500)]
    public string? Reason { get; set; }
}

public class LeaveActionTargetViewModel
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public LeaveType LeaveType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int Days { get; set; }
}

public class RejectLeaveViewModel
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public LeaveType LeaveType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    [Required(ErrorMessage = "Rejection reason is required.")]
    [StringLength(500)]
    [Display(Name = "Reason for Rejection")]
    public string RejectionReason { get; set; } = string.Empty;
}

public class EmployeeLeaveIndexViewModel
{
    public IReadOnlyList<LeaveBalanceCardViewModel> Balances { get; set; } = [];
    public IReadOnlyList<LeaveListItemViewModel> Requests { get; set; } = [];
    public RequestLeaveViewModel RequestForm { get; set; } = new();
    public bool ShowRequestModal { get; set; }
    public int AnnualRemaining { get; set; }
    public int SickRemaining { get; set; }
    public int UsedThisYear { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
}

public class HrLeaveIndexViewModel
{
    public IReadOnlyList<LeaveListItemViewModel> Requests { get; set; } = [];
    public LeaveRequestStatus? StatusFilter { get; set; }
    public LeaveActionTargetViewModel? ApproveTarget { get; set; }
    public RejectLeaveViewModel? RejectForm { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
}

public class DeptHeadLeaveIndexViewModel
{
    public string DepartmentName { get; set; } = string.Empty;
    public IReadOnlyList<LeaveListItemViewModel> Requests { get; set; } = [];
    public LeaveRequestStatus? StatusFilter { get; set; }
    public LeaveActionTargetViewModel? ApproveTarget { get; set; }
    public RejectLeaveViewModel? RejectForm { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
}
