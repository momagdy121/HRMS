using Microsoft.AspNetCore.Http;

namespace HRSystem.Business.DTOs.Recruitment;

public class SubmitJobApplicationDto
{
    public int JobPostingId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? CoverLetter { get; set; }

    public IFormFile CvFile { get; set; } = null!;
}
