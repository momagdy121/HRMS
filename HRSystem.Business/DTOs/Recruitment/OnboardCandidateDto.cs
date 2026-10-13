namespace HRSystem.Business.DTOs.Recruitment;

public class OnboardCandidateDto
{
    public int ApplicationId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public decimal Salary { get; set; }

    public DateOnly HireDate { get; set; }

    public string Role { get; set; } = string.Empty;

    public string InitialPassword { get; set; } = string.Empty;

    public string? LoginUrl { get; set; }
}
