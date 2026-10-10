namespace HRSystem.Web.ViewModels.Dashboard;

public class DashboardActivityItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = string.Empty;
    public string Icon { get; set; } = "info";
    public string IconColorClass { get; set; } = "bg-primary-fixed/30 text-primary-container";
}

public class HrDashboardViewModel
{
    public string FirstName { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public int ActiveDepartments { get; set; }
    public int PendingLeaveRequests { get; set; }
    public decimal MonthlyPayrollTotal { get; set; }
    public IReadOnlyList<DashboardActivityItemViewModel> RecentActivities { get; set; } = [];
}

public class EmployeeDashboardPayslipViewModel
{
    public int Id { get; set; }
    public string Period { get; set; } = string.Empty;
    public string PaymentDate { get; set; } = string.Empty;
    public decimal NetSalary { get; set; }
}

public class EmployeeDashboardTeamMemberViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public bool IsPresentToday { get; set; }
    public string StatusText { get; set; } = "Offline";
}

public class EmployeeDashboardViewModel
{
    public string FirstName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int ActiveTasks { get; set; }
    public int TasksDueSoon { get; set; }
    public int LeaveBalanceDays { get; set; }
    public int DaysPresentThisMonth { get; set; }
    public decimal LastNetSalary { get; set; }
    public IReadOnlyList<EmployeeDashboardPayslipViewModel> RecentPayslips { get; set; } = [];
    public IReadOnlyList<EmployeeDashboardTeamMemberViewModel> TeamMembers { get; set; } = [];
}

public class DepartmentTeamMemberViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class DepartmentHeadDashboardViewModel
{
    public string DepartmentName { get; set; } = string.Empty;
    public int ActiveEmployees { get; set; }
    public int PendingLeaveRequests { get; set; }
    public int PresentThisMonth { get; set; }
    public int AttendancePercent { get; set; }
    public int OverdueTasks { get; set; }
    public IReadOnlyList<DashboardActivityItemViewModel> RecentActivities { get; set; } = [];
    public IReadOnlyList<DepartmentTeamMemberViewModel> TeamMembers { get; set; } = [];
}
