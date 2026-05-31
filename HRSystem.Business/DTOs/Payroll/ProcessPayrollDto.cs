namespace HRSystem.Business.DTOs.Payroll;

public class ProcessPayrollDto
{
    public int EmployeeId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal BaseSalary { get; set; }
}
