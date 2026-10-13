using Microsoft.AspNetCore.Http;

namespace HRSystem.Business.Interfaces.Services;

public interface ICloudinaryService
{
    Task<string?> UploadImageAsync(IFormFile file, string folder = "hrms/avatars", CancellationToken cancellationToken = default);

    Task<string?> UploadImageAsync(Stream stream, string fileName, string folder = "hrms/avatars", CancellationToken cancellationToken = default);

    Task<string?> UploadFileAsync(IFormFile file, string folder = "hrms/documents", CancellationToken cancellationToken = default);

    Task<string?> UploadFileAsync(Stream stream, string fileName, string folder = "hrms/documents", CancellationToken cancellationToken = default);

    Task<HRSystem.Business.DTOs.DocumentUploadResult> UploadDocumentAsync(IFormFile file, string folder = "hrms/cvs", CancellationToken cancellationToken = default);

    Task<(Stream Stream, string ContentType, string FileName)?> GetDocumentStreamAsync(string publicId, string storageUrl, string originalFileName, string contentType, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string publicId, CancellationToken cancellationToken = default);
}
