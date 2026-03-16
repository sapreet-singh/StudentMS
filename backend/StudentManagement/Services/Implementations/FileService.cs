using StudentManagement.DTOs.Responses;
using StudentManagement.Services.Interfaces;

namespace StudentManagement.Services.Implementations;

public class FileService : IFileService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public FileService(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }

    public async Task<ApiResponse<string>> UploadFileAsync(IFormFile file)
    {
        var maxSize = long.Parse(_configuration["FileSettings:MaxFileSizeBytes"]!);
        if (file.Length > maxSize)
            return ApiResponse<string>.Fail($"File size exceeds {maxSize / 1024 / 1024}MB limit.");

        var allowedExtensions = _configuration.GetSection("FileSettings:AllowedExtensions").Get<string[]>()
            ?? new[] { ".jpg", ".jpeg", ".png" };

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
            return ApiResponse<string>.Fail("Invalid file type. Only JPG and PNG are allowed.");

        var uploadPath = Path.Combine(_env.ContentRootPath, _configuration["FileSettings:UploadPath"]!);
        Directory.CreateDirectory(uploadPath);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return ApiResponse<string>.Ok(fileName, "File uploaded successfully.");
    }

    public void DeleteFile(string fileName)
    {
        var uploadPath = Path.Combine(_env.ContentRootPath, _configuration["FileSettings:UploadPath"]!);
        var filePath = Path.Combine(uploadPath, fileName);
        if (File.Exists(filePath))
            File.Delete(filePath);
    }
}
