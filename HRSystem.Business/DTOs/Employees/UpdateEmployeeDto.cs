namespace HRSystem.Business.DTOs.Employees;

public class UpdateEmployeeDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public decimal Salary { get; set; }
    public DateOnly HireDate { get; set; }
    public bool IsActive { get; set; }
}
