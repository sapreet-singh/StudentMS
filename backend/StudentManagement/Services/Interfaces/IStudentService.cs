using StudentManagement.DTOs.Requests;
using StudentManagement.DTOs.Responses;

namespace StudentManagement.Services.Interfaces;

public interface IStudentService
{
    Task<ApiResponse<PagedResponse<StudentResponse>>> GetStudentsAsync(StudentFilterRequest filter);
    Task<ApiResponse<StudentResponse>> GetStudentByIdAsync(int id);
    Task<ApiResponse<StudentResponse>> CreateStudentAsync(CreateStudentRequest request, IFormFile? photo);
    Task<ApiResponse<StudentResponse>> UpdateStudentAsync(int id, UpdateStudentRequest request, IFormFile? photo);
    Task<ApiResponse<bool>> DeleteStudentAsync(int id);
    Task<ApiResponse<StudentResponse>> UpdatePhotoAsync(int id, IFormFile photo);
    Task<IEnumerable<StudentResponse>> GetAllStudentsForExportAsync();
}
