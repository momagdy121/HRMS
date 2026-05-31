namespace HRSystem.Business.DTOs.UserAccounts;

public class UserAccountListItemDto
{
    public int UserId { get; set; }

    public int EmployeeId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool IsPasswordChangeRequired { get; set; }

    public bool IsActive { get; set; }

    public bool IsEmployeeDeleted { get; set; }
}
