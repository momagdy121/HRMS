using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HRSystem.Business.DTOs;
using HRSystem.Business.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HRSystem.Business.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly CloudinarySettings _settings;
    private readonly IWebHostEnvironment? _environment;
    private readonly ILogger<CloudinaryService> _logger;
    private readonly Cloudinary? _cloudinary;

    public CloudinaryService(
        IOptions<CloudinarySettings> options,
        ILogger<CloudinaryService> logger,
        IWebHostEnvironment? environment = null)
    {
        _settings = options.Value;
        _logger = logger;
        _environment = environment;

        if (_settings.IsConfigured)
        {
            try
            {
                var account = new Account(
                    _settings.CloudName,
                    _settings.ApiKey,
                    _settings.ApiSecret);

                _cloudinary = new Cloudinary(account);
                _cloudinary.Api.Secure = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Cloudinary client with configured credentials.");
            }
        }
        else
        {
            _logger.LogInformation("Cloudinary is not configured in appsettings.json. Local storage fallback will be used for uploaded assets.");
        }
    }

    public async Task<string?> UploadImageAsync(IFormFile file, string folder = "hrms/avatars", CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return null;

        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        var bytes = memoryStream.ToArray();

        return await UploadImageBytesAsync(bytes, file.FileName, folder, cancellationToken);
    }

    public async Task<string?> UploadImageAsync(Stream stream, string fileName, string folder = "hrms/avatars", CancellationToken cancellationToken = default)
    {
        if (stream == null)
            return null;

        byte[] bytes;
        if (stream is MemoryStream ms)
        {
            bytes = ms.ToArray();
        }
        else
        {
            await using var mem = new MemoryStream();
            await stream.CopyToAsync(mem, cancellationToken);
            bytes = mem.ToArray();
        }

        return await UploadImageBytesAsync(bytes, fileName, folder, cancellationToken);
    }

    public async Task<string?> UploadFileAsync(IFormFile file, string folder = "hrms/documents", CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return null;

        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        var bytes = memoryStream.ToArray();

        return await UploadFileBytesAsync(bytes, file.FileName, folder, cancellationToken);
    }

    public async Task<string?> UploadFileAsync(Stream stream, string fileName, string folder = "hrms/documents", CancellationToken cancellationToken = default)
    {
        if (stream == null)
            return null;

        byte[] bytes;
        if (stream is MemoryStream ms)
        {
            bytes = ms.ToArray();
        }
        else
        {
            await using var mem = new MemoryStream();
            await stream.CopyToAsync(mem, cancellationToken);
            bytes = mem.ToArray();
        }

        return await UploadFileBytesAsync(bytes, fileName, folder, cancellationToken);
    }

    private async Task<string?> UploadImageBytesAsync(byte[] bytes, string fileName, string folder, CancellationToken cancellationToken)
    {
        if (bytes.Length == 0)
            return null;

        if (_cloudinary != null && _settings.IsConfigured)
        {
            try
            {
                using var uploadStream = new MemoryStream(bytes);
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, uploadStream),
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };

                var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
                if (result.Error != null)
                {
                    _logger.LogWarning("Cloudinary image upload error: {Message}. Falling back to local storage.", result.Error.Message);
                    return await FallbackLocalUploadAsync(bytes, fileName, folder, cancellationToken);
                }

                return result.SecureUrl?.ToString() ?? result.Url?.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cloudinary image upload failed, falling back to local file storage.");
                return await FallbackLocalUploadAsync(bytes, fileName, folder, cancellationToken);
            }
        }

        return await FallbackLocalUploadAsync(bytes, fileName, folder, cancellationToken);
    }

    private async Task<string?> UploadFileBytesAsync(byte[] bytes, string fileName, string folder, CancellationToken cancellationToken)
    {
        if (bytes.Length == 0)
            return null;

        if (_cloudinary != null && _settings.IsConfigured)
        {
            try
            {
                using var uploadStream = new MemoryStream(bytes);
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(fileName, uploadStream),
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = true
                };

                var result = await _cloudinary.UploadAsync(uploadParams, "upload", cancellationToken);
                if (result.Error != null)
                {
                    _logger.LogWarning("Cloudinary raw file upload error: {Message}. Falling back to local storage.", result.Error.Message);
                    return await FallbackLocalUploadAsync(bytes, fileName, folder, cancellationToken);
                }

                return result.SecureUrl?.ToString() ?? result.Url?.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cloudinary raw file upload failed, falling back to local storage.");
                return await FallbackLocalUploadAsync(bytes, fileName, folder, cancellationToken);
            }
        }

        return await FallbackLocalUploadAsync(bytes, fileName, folder, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return false;

        if (_cloudinary != null && _settings.IsConfigured)
        {
            try
            {
                var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));
                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete asset from Cloudinary: {PublicId}", publicId);
                return false;
            }
        }

        return false;
    }

    private async Task<string> FallbackLocalUploadAsync(byte[] bytes, string fileName, string folder, CancellationToken cancellationToken)
    {
        var webRoot = _environment?.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        var normalizedFolder = folder.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        var uploadsDir = Path.Combine(webRoot, "uploads", normalizedFolder);
        Directory.CreateDirectory(uploadsDir);

        var safeFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var targetPath = Path.Combine(uploadsDir, safeFileName);

        await File.WriteAllBytesAsync(targetPath, bytes, cancellationToken);

        var relativeUrl = $"/uploads/{folder.Trim('/')}/{safeFileName}".Replace('\\', '/');
        return relativeUrl;
    }
}
