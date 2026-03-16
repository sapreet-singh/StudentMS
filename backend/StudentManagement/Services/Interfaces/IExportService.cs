namespace StudentManagement.Services.Interfaces;

public interface IExportService
{
    Task<byte[]> ExportStudentsToExcelAsync();
    Task<byte[]> ExportStudentsToPdfAsync();
}
