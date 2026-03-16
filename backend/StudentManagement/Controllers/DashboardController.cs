using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Services.Interfaces;

namespace StudentManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService) => _dashboardService = dashboardService;

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _dashboardService.GetStatsAsync();
        return Ok(result);
    }

    [HttpGet("enrollments-by-month")]
    public async Task<IActionResult> GetEnrollmentsByMonth()
    {
        var result = await _dashboardService.GetEnrollmentsByMonthAsync();
        return Ok(result);
    }

    [HttpGet("students-by-course")]
    public async Task<IActionResult> GetStudentsByCourse()
    {
        var result = await _dashboardService.GetStudentsByCourseAsync();
        return Ok(result);
    }

    [HttpGet("recent-enrollments")]
    public async Task<IActionResult> GetRecentEnrollments([FromQuery] int count = 5)
    {
        var result = await _dashboardService.GetRecentEnrollmentsAsync(count);
        return Ok(result);
    }
}
