using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Services.Interfaces;

namespace StudentManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExportController : ControllerBase
{
    private readonly IExportService _exportService;

    public ExportController(IExportService exportService) => _exportService = exportService;

    [HttpGet("students/excel")]
    public async Task<IActionResult> ExportExcel()
    {
        var bytes = await _exportService.ExportStudentsToExcelAsync();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"students_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    [HttpGet("students/pdf")]
    public async Task<IActionResult> ExportPdf()
    {
        var bytes = await _exportService.ExportStudentsToPdfAsync();
        return File(bytes, "application/pdf", $"students_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
    }
}
