using Microsoft.AspNetCore.Http;

namespace HRSystem.Business.Helpers;

public static class CvFileValidator
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".doc",
        ".docx"
    };

    private static readonly byte[] PdfHeader = [0x25, 0x50, 0x44, 0x46]; // %PDF
    private static readonly byte[] DocHeader = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1]; // OLE2
    private static readonly byte[] DocxZipHeader1 = [0x50, 0x4B, 0x03, 0x04]; // PK..
    private static readonly byte[] DocxZipHeader2 = [0x50, 0x4B, 0x05, 0x06];
    private static readonly byte[] DocxZipHeader3 = [0x50, 0x4B, 0x07, 0x08];

    public static (bool IsValid, string? ErrorMessage) Validate(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return (false, "Please upload your CV document.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return (false, "The CV file size exceeds the 5 MB limit.");
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
        {
            return (false, "Only PDF, DOC, and DOCX file formats are allowed.");
        }

        try
        {
            using var stream = file.OpenReadStream();
            var headerBytes = new byte[8];
            var bytesRead = stream.Read(headerBytes, 0, headerBytes.Length);

            if (bytesRead < 4)
            {
                return (false, "The uploaded file is empty or corrupted.");
            }

            if (ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                if (!StartsWith(headerBytes, PdfHeader))
                {
                    return (false, "The uploaded file does not have a valid PDF signature.");
                }
            }
            else if (ext.Equals(".doc", StringComparison.OrdinalIgnoreCase))
            {
                if (bytesRead < 8 || !StartsWith(headerBytes, DocHeader))
                {
                    return (false, "The uploaded file does not have a valid DOC signature.");
                }
            }
            else if (ext.Equals(".docx", StringComparison.OrdinalIgnoreCase))
            {
                if (!StartsWith(headerBytes, DocxZipHeader1) &&
                    !StartsWith(headerBytes, DocxZipHeader2) &&
                    !StartsWith(headerBytes, DocxZipHeader3))
                {
                    return (false, "The uploaded file does not have a valid DOCX package signature.");
                }
            }
        }
        catch
        {
            return (false, "Unable to read the uploaded CV file.");
        }

        return (true, null);
    }

    private static bool StartsWith(byte[] source, byte[] prefix)
    {
        if (source.Length < prefix.Length) return false;
        for (var i = 0; i < prefix.Length; i++)
        {
            if (source[i] != prefix[i]) return false;
        }
        return true;
    }
}
