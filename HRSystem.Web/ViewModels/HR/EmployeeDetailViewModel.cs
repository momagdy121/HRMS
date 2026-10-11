using HRSystem.Common.Enums;

namespace HRSystem.Web.ViewModels.HR;

public class EmployeeDetailViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Initials { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public bool IsManager { get; set; }
    public bool IsHR { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateOnly HireDate { get; set; }
    public decimal Salary { get; set; }
    public string? ManagerName { get; set; }
    public int? ManagerId { get; set; }
    public string Tenure { get; set; } = string.Empty;

    // Leave Balances
    public IReadOnlyList<EmployeeLeaveBalanceItemViewModel> LeaveBalances { get; set; } = [];
    public IReadOnlyList<EmployeeRecentLeaveViewModel> RecentLeaves { get; set; } = [];

    // Payroll info
    public IReadOnlyList<EmployeeRecentPayrollViewModel> RecentPayrolls { get; set; } = [];

    // Attendance info
    public IReadOnlyList<EmployeeRecentAttendanceViewModel> RecentAttendances { get; set; } = [];
    public int DaysPresentThisMonth { get; set; }
    public int AbsencesThisMonth { get; set; }

    // Task info
    public IReadOnlyList<EmployeeRecentTaskViewModel> AssignedTasks { get; set; } = [];
    public int ActiveTasksCount { get; set; }

    public EditEmployeeViewModel? EditForm { get; set; }
}

public class EmployeeLeaveBalanceItemViewModel
{
    public LeaveType LeaveType { get; set; }
    public int TotalDays { get; set; }
    public int UsedDays { get; set; }
    public int RemainingDays => TotalDays - UsedDays;
}

public class EmployeeRecentLeaveViewModel
{
    public int Id { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int DaysCount { get; set; }
    public LeaveRequestStatus Status { get; set; }
    public string? Reason { get; set; }
}

public class EmployeeRecentPayrollViewModel
{
    public int Id { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public decimal TotalBonus { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal NetSalary { get; set; }
    public PayrollStatus Status { get; set; }
}

public class EmployeeRecentAttendanceViewModel
{
    public DateOnly Date { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string DurationLabel { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
}

public class EmployeeRecentTaskViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly? DueDate { get; set; }
    public bool IsCompleted { get; set; }
}
