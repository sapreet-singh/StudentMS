using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.DTOs.Responses;
using StudentManagement.Services.Interfaces;

namespace StudentManagement.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<DashboardStats>> GetStatsAsync()
    {
        var now = DateTime.UtcNow;
        var stats = new DashboardStats
        {
            TotalStudents = await _context.Students.CountAsync(),
            ActiveStudents = await _context.Students.CountAsync(s => s.IsActive),
            TotalCourses = await _context.Courses.CountAsync(),
            NewEnrollmentsThisMonth = await _context.Students.CountAsync(s =>
                s.EnrollmentDate.Year == now.Year && s.EnrollmentDate.Month == now.Month)
        };
        return ApiResponse<DashboardStats>.Ok(stats);
    }

    public async Task<ApiResponse<List<EnrollmentByMonth>>> GetEnrollmentsByMonthAsync()
    {
        var cutoff = DateTime.UtcNow.AddMonths(-11);
        var start = new DateTime(cutoff.Year, cutoff.Month, 1);

        var data = await _context.Students
            .Where(s => s.EnrollmentDate >= start)
            .GroupBy(s => new { s.EnrollmentDate.Year, s.EnrollmentDate.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Count = g.Count()
            })
            .OrderBy(e => e.Year).ThenBy(e => e.Month)
            .ToListAsync();

        var enrollments = data.Select(d => new EnrollmentByMonth
        {
            Year = d.Year,
            Month = d.Month.ToString("00"),
            Count = d.Count
        }).ToList();

        // Fill in missing months with 0
        var result = new List<EnrollmentByMonth>();
        for (int i = 0; i < 12; i++)
        {
            var date = start.AddMonths(i);
            var existing = enrollments.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month.ToString("00"));
            result.Add(new EnrollmentByMonth
            {
                Year = date.Year,
                Month = date.ToString("MMM yyyy"),
                Count = existing?.Count ?? 0
            });
        }

        return ApiResponse<List<EnrollmentByMonth>>.Ok(result);
    }

    public async Task<ApiResponse<List<StudentsByCourse>>> GetStudentsByCourseAsync()
    {
        var data = await _context.Students
            .Include(s => s.Course)
            .GroupBy(s => s.Course.Name)
            .Select(g => new StudentsByCourse { CourseName = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        return ApiResponse<List<StudentsByCourse>>.Ok(data);
    }

    public async Task<ApiResponse<List<StudentResponse>>> GetRecentEnrollmentsAsync(int count = 5)
    {
        var students = await _context.Students
            .Include(s => s.Course)
            .OrderByDescending(s => s.EnrollmentDate)
            .Take(count)
            .Select(s => new StudentResponse
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
                Phone = s.Phone,
                DateOfBirth = s.DateOfBirth,
                EnrollmentDate = s.EnrollmentDate,
                CourseId = s.CourseId,
                CourseName = s.Course.Name,
                IsActive = s.IsActive,
                PhotoPath = s.PhotoPath,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<StudentResponse>>.Ok(students);
    }
}
