namespace HRSystem.Business.DTOs;

public class DocumentUploadResult
{
    public bool Succeeded { get; set; }
    public string? ErrorMessage { get; set; }
    public string PublicId { get; set; } = string.Empty;
    public string StorageUrl { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ResourceType { get; set; } = "raw";
    public bool IsLocalStorage { get; set; }
    public string? LocalFilePath { get; set; }
}
