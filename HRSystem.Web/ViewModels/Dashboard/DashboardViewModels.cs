namespace HRSystem.Web.ViewModels.Dashboard;

public class HrDashboardViewModel
{
    public string FirstName { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public int ActiveDepartments { get; set; }
    public int PendingLeaveRequests { get; set; }
    public decimal MonthlyPayrollTotal { get; set; }
}

public class EmployeeDashboardViewModel
{
    public string FirstName { get; set; } = string.Empty;
    public int ActiveTasks { get; set; }
    public int TasksDueSoon { get; set; }
    public int LeaveBalanceDays { get; set; }
    public int DaysPresentThisMonth { get; set; }
    public decimal LastNetSalary { get; set; }
}

public class DepartmentHeadDashboardViewModel
{
    public string DepartmentName { get; set; } = string.Empty;
    public int ActiveEmployees { get; set; }
    public int PendingLeaveRequests { get; set; }
    public int PresentThisMonth { get; set; }
    public int AttendancePercent { get; set; }
    public int OverdueTasks { get; set; }
}
