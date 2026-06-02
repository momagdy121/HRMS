namespace HRSystem.Web.ViewModels.HR;

public class EmployeeListItemViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public bool IsManager { get; set; }
    public DateOnly HireDate { get; set; }
    public bool CanDelete { get; set; }
    public string Initials { get; set; } = string.Empty;
}

public class EmployeeIndexViewModel
{
    public IReadOnlyList<EmployeeListItemViewModel> Employees { get; set; } = [];
    public IReadOnlyList<DepartmentOptionViewModel> Departments { get; set; } = [];
    public CreateEmployeeViewModel CreateForm { get; set; } = new();
    public EditEmployeeViewModel? EditForm { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public int? DepartmentFilter { get; set; }
    public bool ShowCreateModal { get; set; }
}

public class DepartmentOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ManagerOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
