using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.DTOs.Requests;
using StudentManagement.DTOs.Responses;
using StudentManagement.Helpers;
using StudentManagement.Models;
using StudentManagement.Services.Interfaces;

namespace StudentManagement.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtHelper _jwtHelper;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, JwtHelper jwtHelper, IConfiguration configuration)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _configuration = configuration;
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return ApiResponse<AuthResponse>.Fail("Invalid username or password.");

        var token = _jwtHelper.GenerateToken(user);
        var refreshToken = _jwtHelper.GenerateRefreshToken();
        var expiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"]!);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(expiryDays);
        await _context.SaveChangesAsync();

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(int.Parse(_configuration["JwtSettings:ExpirationHours"]!)),
            User = new UserInfo { Id = user.Id, Username = user.Username, Email = user.Email, Role = user.Role.Name }
        }, "Login successful.");
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return ApiResponse<AuthResponse>.Fail("Username already exists.");

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return ApiResponse<AuthResponse>.Fail("Email already registered.");

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == request.RoleName)
            ?? await _context.Roles.FirstAsync(r => r.Name == "Student");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = role.Id
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await _context.Entry(user).Reference(u => u.Role).LoadAsync();

        var token = _jwtHelper.GenerateToken(user);
        var refreshToken = _jwtHelper.GenerateRefreshToken();
        var expiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"]!);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(expiryDays);
        await _context.SaveChangesAsync();

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(int.Parse(_configuration["JwtSettings:ExpirationHours"]!)),
            User = new UserInfo { Id = user.Id, Username = user.Username, Email = user.Email, Role = user.Role.Name }
        }, "Registration successful.");
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshToken)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiry > DateTime.UtcNow);

        if (user == null)
            return ApiResponse<AuthResponse>.Fail("Invalid or expired refresh token.");

        var newToken = _jwtHelper.GenerateToken(user);
        var newRefreshToken = _jwtHelper.GenerateRefreshToken();
        var expiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"]!);

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(expiryDays);
        await _context.SaveChangesAsync();

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(int.Parse(_configuration["JwtSettings:ExpirationHours"]!)),
            User = new UserInfo { Id = user.Id, Username = user.Username, Email = user.Email, Role = user.Role.Name }
        });
    }

    public async Task<ApiResponse<bool>> RevokeTokenAsync(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return ApiResponse<bool>.Fail("User not found.");

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Token revoked.");
    }
}
