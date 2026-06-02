using System.ComponentModel.DataAnnotations;

namespace HRSystem.Web.ViewModels.HR;

public class CreateDepartmentViewModel
{
    [Required(ErrorMessage = "Department name is required.")]
    [StringLength(100)]
    [Display(Name = "Department Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Manager is required.")]
    [Display(Name = "Manager")]
    public int ManagerId { get; set; }
}

public class EditDepartmentViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Department name is required.")]
    [StringLength(100)]
    [Display(Name = "Department Name")]
    public string Name { get; set; } = string.Empty;
}

public class ReplaceManagerViewModel
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select a new manager.")]
    [Display(Name = "New Manager")]
    public int NewManagerId { get; set; }
}
