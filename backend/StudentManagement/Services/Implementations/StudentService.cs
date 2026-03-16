using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.DTOs.Requests;
using StudentManagement.DTOs.Responses;
using StudentManagement.Models;
using StudentManagement.Services.Interfaces;

namespace StudentManagement.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly AppDbContext _context;
    private readonly IFileService _fileService;

    public StudentService(AppDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    public async Task<ApiResponse<PagedResponse<StudentResponse>>> GetStudentsAsync(StudentFilterRequest filter)
    {
        var query = _context.Students.Include(s => s.Course).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(s =>
                s.FirstName.ToLower().Contains(search) ||
                s.LastName.ToLower().Contains(search) ||
                s.Email.ToLower().Contains(search));
        }

        if (filter.CourseId.HasValue)
            query = query.Where(s => s.CourseId == filter.CourseId.Value);

        if (filter.IsActive.HasValue)
            query = query.Where(s => s.IsActive == filter.IsActive.Value);

        var totalCount = await query.CountAsync();
        var students = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(s => MapToResponse(s))
            .ToListAsync();

        return ApiResponse<PagedResponse<StudentResponse>>.Ok(new PagedResponse<StudentResponse>
        {
            Items = students,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        });
    }

    public async Task<ApiResponse<StudentResponse>> GetStudentByIdAsync(int id)
    {
        var student = await _context.Students.Include(s => s.Course).FirstOrDefaultAsync(s => s.Id == id);
        if (student == null) return ApiResponse<StudentResponse>.Fail("Student not found.");
        return ApiResponse<StudentResponse>.Ok(MapToResponse(student));
    }

    public async Task<ApiResponse<StudentResponse>> CreateStudentAsync(CreateStudentRequest request, IFormFile? photo)
    {
        if (await _context.Students.AnyAsync(s => s.Email == request.Email))
            return ApiResponse<StudentResponse>.Fail("Email already exists.");

        if (!await _context.Courses.AnyAsync(c => c.Id == request.CourseId))
            return ApiResponse<StudentResponse>.Fail("Invalid course.");

        string? photoPath = null;
        if (photo != null)
        {
            var uploadResult = await _fileService.UploadFileAsync(photo);
            if (!uploadResult.Success) return ApiResponse<StudentResponse>.Fail(uploadResult.Message);
            photoPath = uploadResult.Data;
        }

        var student = new Student
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            EnrollmentDate = request.EnrollmentDate,
            CourseId = request.CourseId,
            IsActive = request.IsActive,
            PhotoPath = photoPath,
            CreatedAt = DateTime.UtcNow
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        await _context.Entry(student).Reference(s => s.Course).LoadAsync();

        return ApiResponse<StudentResponse>.Ok(MapToResponse(student), "Student created successfully.");
    }

    public async Task<ApiResponse<StudentResponse>> UpdateStudentAsync(int id, UpdateStudentRequest request, IFormFile? photo)
    {
        var student = await _context.Students.Include(s => s.Course).FirstOrDefaultAsync(s => s.Id == id);
        if (student == null) return ApiResponse<StudentResponse>.Fail("Student not found.");

        if (await _context.Students.AnyAsync(s => s.Email == request.Email && s.Id != id))
            return ApiResponse<StudentResponse>.Fail("Email already used by another student.");

        if (!await _context.Courses.AnyAsync(c => c.Id == request.CourseId))
            return ApiResponse<StudentResponse>.Fail("Invalid course.");

        if (photo != null)
        {
            if (student.PhotoPath != null)
                _fileService.DeleteFile(student.PhotoPath);

            var uploadResult = await _fileService.UploadFileAsync(photo);
            if (!uploadResult.Success) return ApiResponse<StudentResponse>.Fail(uploadResult.Message);
            student.PhotoPath = uploadResult.Data;
        }

        student.FirstName = request.FirstName;
        student.LastName = request.LastName;
        student.Email = request.Email;
        student.Phone = request.Phone;
        student.DateOfBirth = request.DateOfBirth;
        student.EnrollmentDate = request.EnrollmentDate;
        student.CourseId = request.CourseId;
        student.IsActive = request.IsActive;

        await _context.SaveChangesAsync();
        await _context.Entry(student).Reference(s => s.Course).LoadAsync();

        return ApiResponse<StudentResponse>.Ok(MapToResponse(student), "Student updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteStudentAsync(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null) return ApiResponse<bool>.Fail("Student not found.");

        if (student.PhotoPath != null)
            _fileService.DeleteFile(student.PhotoPath);

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Student deleted successfully.");
    }

    public async Task<ApiResponse<StudentResponse>> UpdatePhotoAsync(int id, IFormFile photo)
    {
        var student = await _context.Students.Include(s => s.Course).FirstOrDefaultAsync(s => s.Id == id);
        if (student == null) return ApiResponse<StudentResponse>.Fail("Student not found.");

        if (student.PhotoPath != null)
            _fileService.DeleteFile(student.PhotoPath);

        var uploadResult = await _fileService.UploadFileAsync(photo);
        if (!uploadResult.Success) return ApiResponse<StudentResponse>.Fail(uploadResult.Message);

        student.PhotoPath = uploadResult.Data;
        await _context.SaveChangesAsync();

        return ApiResponse<StudentResponse>.Ok(MapToResponse(student), "Photo updated successfully.");
    }

    public async Task<IEnumerable<StudentResponse>> GetAllStudentsForExportAsync()
    {
        return await _context.Students
            .Include(s => s.Course)
            .OrderBy(s => s.LastName)
            .Select(s => MapToResponse(s))
            .ToListAsync();
    }

    private static StudentResponse MapToResponse(Student s) => new()
    {
        Id = s.Id,
        FirstName = s.FirstName,
        LastName = s.LastName,
        Email = s.Email,
        Phone = s.Phone,
        DateOfBirth = s.DateOfBirth,
        EnrollmentDate = s.EnrollmentDate,
        CourseId = s.CourseId,
        CourseName = s.Course?.Name ?? string.Empty,
        IsActive = s.IsActive,
        PhotoPath = s.PhotoPath,
        CreatedAt = s.CreatedAt
    };
}
