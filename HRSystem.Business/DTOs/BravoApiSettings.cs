namespace HRSystem.Business.DTOs;

public class BravoApiSettings
{
    public const string SectionName = "BravoApi";

    public string ApiKey { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
}
