namespace HRSystem.Business.DTOs.Departments;

public class CreateDepartmentDto
{
    public string Name { get; set; } = string.Empty;
    public int ManagerId { get; set; }
}
