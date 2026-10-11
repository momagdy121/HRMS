namespace HRSystem.Web.ViewModels.HR;

public class DepartmentDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public int? ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public string ManagerInitials { get; set; } = string.Empty;
    public string ManagerEmail { get; set; } = string.Empty;
    public string? ManagerPhone { get; set; }
    public string? ManagerAvatarUrl { get; set; }
    public DateOnly? ManagerHireDate { get; set; }
    public bool HasManager => ManagerId.HasValue && ManagerId.Value > 0;

    public int TotalEmployees { get; set; }
    public decimal TotalSalaryBudget { get; set; }
    public decimal AverageSalary { get; set; }

    public IReadOnlyList<DepartmentEmployeeItemViewModel> Employees { get; set; } = [];
    public IReadOnlyList<ManagerOptionViewModel> ManagerOptions { get; set; } = [];
    public EditDepartmentViewModel? EditForm { get; set; }
    public ReplaceManagerViewModel? ReplaceForm { get; set; }
}

public class DepartmentEmployeeItemViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public decimal Salary { get; set; }
    public DateOnly HireDate { get; set; }
    public bool IsManager { get; set; }
    public bool IsHR { get; set; }
    public bool IsActive { get; set; }
}
