using System.ComponentModel.DataAnnotations;
using HRSystem.Common.Enums;

namespace HRSystem.Web.ViewModels.Payroll;

public class PayrollListItemViewModel
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeInitials { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal TotalBonus { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal NetSalary { get; set; }
    public PayrollStatus Status { get; set; }
}

public class ProcessPayrollViewModel
{
    [Required(ErrorMessage = "Please select an employee.")]
    [Display(Name = "Employee")]
    public int EmployeeId { get; set; }

    [Required]
    [Range(1, 12)]
    [Display(Name = "Month")]
    public int Month { get; set; } = DateTime.UtcNow.Month;

    [Required]
    [Range(2000, 2100)]
    [Display(Name = "Year")]
    public int Year { get; set; } = DateTime.UtcNow.Year;
}

public class AddPayrollItemViewModel
{
    public int PayrollId { get; set; }

    [Required]
    [Display(Name = "Type")]
    public ItemType ItemType { get; set; } = ItemType.Bonus;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }
}

public class EditPayrollItemViewModel
{
    public int Id { get; set; }
    public int PayrollId { get; set; }

    [Required]
    [Display(Name = "Type")]
    public ItemType ItemType { get; set; } = ItemType.Bonus;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }
}

public class RemovePayrollItemViewModel
{
    public int Id { get; set; }
    public int PayrollId { get; set; }
    public ItemType ItemType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class PayrollItemRowViewModel
{
    public int Id { get; set; }
    public ItemType ItemType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class HrPayrollIndexViewModel
{
    public IReadOnlyList<PayrollListItemViewModel> Payrolls { get; set; } = [];
    public IReadOnlyList<DepartmentFilterOptionViewModel> Departments { get; set; } = [];
    public IReadOnlyList<EmployeeFilterOptionViewModel> Employees { get; set; } = [];
    public ProcessPayrollViewModel ProcessForm { get; set; } = new();
    public int? DepartmentFilter { get; set; }
    public int? MonthFilter { get; set; }
    public int? YearFilter { get; set; }
    public PayrollStatus? StatusFilter { get; set; }
    public bool ShowProcessModal { get; set; }
    public decimal TotalNetForPeriod { get; set; }
    public int PendingApprovalCount { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
}

public class HrPayrollDetailViewModel
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal TotalBonus { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal NetSalary { get; set; }
    public PayrollStatus Status { get; set; }
    public IReadOnlyList<PayrollItemRowViewModel> Items { get; set; } = [];
    public AddPayrollItemViewModel AddItemForm { get; set; } = new();
    public EditPayrollItemViewModel? EditItemForm { get; set; }
    public RemovePayrollItemViewModel? RemoveItemTarget { get; set; }
    public bool CanManageItems { get; set; }
    public bool CanAddItems { get; set; }
    public bool CanApprove { get; set; }
    public bool CanMarkPaid { get; set; }
}

public class PayslipViewModel
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal TotalBonus { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal NetSalary { get; set; }
    public PayrollStatus Status { get; set; }
    public IReadOnlyList<PayrollItemRowViewModel> Items { get; set; } = [];
    public bool ShowLineItems { get; set; } = true;
}

public class EmployeePayrollIndexViewModel
{
    public IReadOnlyList<PayrollListItemViewModel> Payrolls { get; set; } = [];
    public PayslipViewModel? PayslipDetail { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
}

public class DeptHeadPayrollIndexViewModel
{
    public string DepartmentName { get; set; } = string.Empty;
    public IReadOnlyList<PayrollListItemViewModel> Payrolls { get; set; } = [];
    public PayslipViewModel? PayslipDetail { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
}

public class DepartmentFilterOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class EmployeeFilterOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
