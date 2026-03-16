using StudentManagement.DTOs.Responses;

namespace StudentManagement.Services.Interfaces;

public interface IFileService
{
    Task<ApiResponse<string>> UploadFileAsync(IFormFile file);
    void DeleteFile(string fileName);
}
