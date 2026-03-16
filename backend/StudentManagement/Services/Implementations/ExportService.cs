using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StudentManagement.Services.Interfaces;

namespace StudentManagement.Services.Implementations;

public class ExportService : IExportService
{
    private readonly IStudentService _studentService;

    public ExportService(IStudentService studentService)
    {
        _studentService = studentService;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> ExportStudentsToExcelAsync()
    {
        var students = (await _studentService.GetAllStudentsForExportAsync()).ToList();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Students");

        // Header row styling
        var headers = new[] { "ID", "First Name", "Last Name", "Email", "Phone", "Date of Birth", "Enrollment Date", "Course", "Status", "Created At" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1976D2");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Data rows
        for (int i = 0; i < students.Count; i++)
        {
            var s = students[i];
            int row = i + 2;
            worksheet.Cell(row, 1).Value = s.Id;
            worksheet.Cell(row, 2).Value = s.FirstName;
            worksheet.Cell(row, 3).Value = s.LastName;
            worksheet.Cell(row, 4).Value = s.Email;
            worksheet.Cell(row, 5).Value = s.Phone;
            worksheet.Cell(row, 6).Value = s.DateOfBirth.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 7).Value = s.EnrollmentDate.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 8).Value = s.CourseName;
            worksheet.Cell(row, 9).Value = s.IsActive ? "Active" : "Inactive";
            worksheet.Cell(row, 10).Value = s.CreatedAt.ToString("yyyy-MM-dd");

            if (i % 2 == 1)
            {
                var rowRange = worksheet.Row(row);
                rowRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F5F5");
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportStudentsToPdfAsync()
    {
        var students = (await _studentService.GetAllStudentsForExportAsync()).ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ctx => ComposeContent(ctx, students));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Student Management System").FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                col.Item().Text($"Student List — Generated: {DateTime.Now:dd MMM yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private void ComposeContent(IContainer container, List<DTOs.Responses.StudentResponse> students)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(30);
                cols.RelativeColumn(2);
                cols.RelativeColumn(2.5f);
                cols.ConstantColumn(70);
                cols.ConstantColumn(70);
                cols.RelativeColumn(2);
                cols.ConstantColumn(55);
            });

            // Header
            static IContainer HeaderCell(IContainer c) =>
                c.DefaultTextStyle(x => x.Bold().FontColor(Colors.White))
                 .PaddingVertical(5).PaddingHorizontal(4)
                 .Background(Colors.Blue.Darken2);

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("#");
                header.Cell().Element(HeaderCell).Text("Full Name");
                header.Cell().Element(HeaderCell).Text("Email");
                header.Cell().Element(HeaderCell).Text("Phone");
                header.Cell().Element(HeaderCell).Text("Course");
                header.Cell().Element(HeaderCell).Text("Enrolled");
                header.Cell().Element(HeaderCell).Text("Status");
            });

            // Rows
            foreach (var (s, idx) in students.Select((s, i) => (s, i)))
            {
                var bg = idx % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                static IContainer DataCell(IContainer c, string bg) =>
                    c.Background(bg).PaddingVertical(4).PaddingHorizontal(4);

                table.Cell().Element(c => DataCell(c, bg)).Text((idx + 1).ToString());
                table.Cell().Element(c => DataCell(c, bg)).Text($"{s.FirstName} {s.LastName}");
                table.Cell().Element(c => DataCell(c, bg)).Text(s.Email);
                table.Cell().Element(c => DataCell(c, bg)).Text(s.Phone);
                table.Cell().Element(c => DataCell(c, bg)).Text(s.CourseName);
                table.Cell().Element(c => DataCell(c, bg)).Text(s.EnrollmentDate.ToString("dd/MM/yyyy"));
                table.Cell().Element(c => DataCell(c, bg))
                    .Text(s.IsActive ? "Active" : "Inactive")
                    .FontColor(s.IsActive ? Colors.Green.Darken2 : Colors.Red.Darken2);
            }
        });
    }
}
