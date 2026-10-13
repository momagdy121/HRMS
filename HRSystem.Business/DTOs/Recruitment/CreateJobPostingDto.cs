namespace HRSystem.Business.DTOs.Recruitment;

public class CreateJobPostingDto
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Requirements { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public DateOnly ClosingDate { get; set; }

    public int CreatedById { get; set; }
}
