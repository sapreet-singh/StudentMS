using StudentManagement.DTOs.Requests;
using StudentManagement.DTOs.Responses;

namespace StudentManagement.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse<bool>> RevokeTokenAsync(string username);
}
