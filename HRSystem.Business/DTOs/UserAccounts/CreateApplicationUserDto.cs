namespace HRSystem.Business.DTOs.UserAccounts;

public class CreateApplicationUserDto
{
    public int EmployeeId { get; set; }

    public string Email { get; set; } = string.Empty;
}
