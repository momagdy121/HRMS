namespace HRSystem.Web.ViewModels.HR;

public class DepartmentListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public string ManagerInitials { get; set; } = string.Empty;
    public bool HasManager { get; set; }
    public int EmployeeCount { get; set; }
}

public class DepartmentIndexViewModel
{
    public IReadOnlyList<DepartmentListItemViewModel> Departments { get; set; } = [];
    public IReadOnlyList<ManagerOptionViewModel> ManagerOptions { get; set; } = [];
    public CreateDepartmentViewModel CreateForm { get; set; } = new();
    public EditDepartmentViewModel? EditForm { get; set; }
    public ReplaceManagerViewModel? ReplaceForm { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public bool ShowCreateModal { get; set; }
    public DeletedDepartmentNameConflictViewModel? DeletedNameConflict { get; set; }
}

public class DeletedDepartmentNameConflictViewModel
{
    public int DeletedDepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? SelectedManagerId { get; set; }
    public bool FromEdit { get; set; }
    public int? EditingDepartmentId { get; set; }
}
