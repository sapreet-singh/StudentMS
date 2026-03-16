using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.DTOs.Requests;
using StudentManagement.Services.Interfaces;

namespace StudentManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService) => _studentService = studentService;

    [HttpGet]
    public async Task<IActionResult> GetStudents([FromQuery] StudentFilterRequest filter)
    {
        var result = await _studentService.GetStudentsAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetStudent(int id)
    {
        var result = await _studentService.GetStudentByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateStudent([FromForm] CreateStudentRequest request, IFormFile? photo)
    {
        var result = await _studentService.CreateStudentAsync(request, photo);
        return result.Success ? CreatedAtAction(nameof(GetStudent), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> UpdateStudent(int id, [FromForm] UpdateStudentRequest request, IFormFile? photo)
    {
        var result = await _studentService.UpdateStudentAsync(id, request, photo);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        var result = await _studentService.DeleteStudentAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id:int}/photo")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> UpdatePhoto(int id, IFormFile photo)
    {
        if (photo == null) return BadRequest("Photo file is required.");
        var result = await _studentService.UpdatePhotoAsync(id, photo);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
