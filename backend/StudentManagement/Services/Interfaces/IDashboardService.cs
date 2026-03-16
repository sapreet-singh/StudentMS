using StudentManagement.DTOs.Responses;

namespace StudentManagement.Services.Interfaces;

public interface IDashboardService
{
    Task<ApiResponse<DashboardStats>> GetStatsAsync();
    Task<ApiResponse<List<EnrollmentByMonth>>> GetEnrollmentsByMonthAsync();
    Task<ApiResponse<List<StudentsByCourse>>> GetStudentsByCourseAsync();
    Task<ApiResponse<List<StudentResponse>>> GetRecentEnrollmentsAsync(int count = 5);
}
