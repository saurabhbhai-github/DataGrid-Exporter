using iText.IO.Image;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using PF_TRANSFER_IN__REGISTER_FORMAT.Data;
using PF_TRANSFER_IN__REGISTER_FORMAT.Models;
using OfficeOpenXml.Style;
using System.Drawing;

namespace PF_TRANSFER_IN__REGISTER_FORMAT.Controllers
{
    public class PFTransferController : Controller
    {
        private readonly PFTransferRepository _dbHelper;
        public PFTransferController(PFTransferRepository repository)
        {
            _dbHelper = repository;
        }

        public IActionResult Index(int pageNumber = 1, int pageSize = 5)
        {
            List<PFSettlementModel> data = _dbHelper.GetPFTransferData();
            if (data == null || data.Count == 0)
            {
                return View(new List<PFSettlementModel>());
            }

            int totalRecords = data.Count;
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Ensure pageNumber doesn't exceed totalPages
            if (pageNumber > totalPages)
            {
                pageNumber = totalPages;
            }

            var paginatedData = data.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;

            return View(paginatedData);
        }

        public IActionResult ExportToExcel()
        {
            var data = _dbHelper.GetPFTransferData();

            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("PF Transfer Register");
                worksheet.Cells["A1"].LoadFromCollection(data, true);

                int totalColumns = worksheet.Dimension.Columns;
                int totalRows = worksheet.Dimension.Rows;

                // Format Header Row (Bold, Centered, Background Color)
                using (var headerRange = worksheet.Cells[1, 1, 1, worksheet.Dimension.Columns])
                {
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                // Center align all data rows
                using (var dataRange = worksheet.Cells[2, 1, totalRows, totalColumns])
                {
                    dataRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; 
                }
                // Auto-fit columns for better readability00
                worksheet.Cells.AutoFitColumns();
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PF_Transfer.xlsx");
            }
        }
        public IActionResult GeneratePdf(int id)
        {
            var data = _dbHelper.GetPFTransferData().Find(x => x.SrNo == id);

            if (data == null) { return NotFound("PF Transfer data not found for the given ID."); }
            using (MemoryStream stream = new MemoryStream())
            {
                // Create PDF writer and document
                using (iText.Kernel.Pdf.PdfWriter writer = new iText.Kernel.Pdf.PdfWriter(stream))
                using (iText.Kernel.Pdf.PdfDocument pdf = new iText.Kernel.Pdf.PdfDocument(writer))
                using (iText.Layout.Document document = new iText.Layout.Document(pdf))
                {
                    document.SetMargins(20, 20, 20, 20);
                    // Add Logo (Assuming "wwwroot/images/logo.png")
                    string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
                    if (System.IO.File.Exists(logoPath))
                    {
                        iText.Layout.Element.Image logo = new iText.Layout.Element.Image(ImageDataFactory.Create(logoPath)).SetWidth(100);
                        document.Add(logo.SetHorizontalAlignment(HorizontalAlignment.CENTER));
                    }

                    // Add Titles
                    document.Add(new Paragraph("Provident Fund Transfer Statement") 
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(16)
                        .SetBold());
                
                    // Create a Table
                    Table table = new Table(2).UseAllAvailableWidth();

                    // Reusable method to add a cell with optional bold
                    void AddCell(Table tbl, string text, bool isBold = false)
                    {
                        var cell = new Cell().Add(new Paragraph(text));
                        if (isBold) cell.SetBold();
                        tbl.AddCell(cell);
                    }

                    // Adding Header and Data Cells
                    AddCell(table, "Sr. No", true); AddCell(table, data.SrNo.ToString() ?? "N/A");
                    AddCell(table, "Emp No", true); AddCell(table, data.EmpNo ?? "N/A");
                    AddCell(table, "PF Number", true); AddCell(table, data.PF_Number ?? "N/A");
                    AddCell(table, "Date of Transfer In", true);
                    AddCell(table, data.Date_Of_Transfer_In.ToString("dd-MM-yyyy") ?? "N/A");

                    AddCell(table, "TRNS Type", true); AddCell(table, data.TRNS_Type ?? "N/A");
                    AddCell(table, "Date of Joining Prior", true);
                    AddCell(table, data.Date_Of_Joining_Prior?.ToString("dd-MM-yyyy") ?? "N/A");

                    AddCell(table, "Name of Member", true); AddCell(table, data.Name_Of_Member ?? "N/A");
                    AddCell(table, "Company Name", true); AddCell(table, data.Company_Name ?? "N/A");
                    AddCell(table, "Trust/RPFC Address-1", true); AddCell(table, data.Trust_RPFC_Address ?? "N/A");
                    AddCell(table, "From PF Account", true); AddCell(table, data.From_PF_Account ?? "N/A" );
                    AddCell(table, "To PF Account", true); AddCell(table, data.To_PF_Account ?? "N/A");

                    AddCell(table, "Employee Contribution Amount", true);
                    AddCell(table, $"{data.Employee_Contb_Amount}" ?? "0");

                    AddCell(table, "Employer Contribution Amount", true);
                    AddCell(table, $"{data.Employer_Contb_Amount}" ?? "0");

                    AddCell(table, "Total Contribution Amount", true);
                    AddCell(table, $"{data.Total_Contb_Amount}" ?? "0");

                    AddCell(table, "Status", true); AddCell(table, data.Status ?? "N/A");
                    AddCell(table, "FI Document Number", true); AddCell(table, data.FI_Document_Number ?? "N/A");

                    Paragraph companyName = new Paragraph("Mahindra & Mahindra")
                        .SetBold()  
                        .SetTextAlignment(TextAlignment.RIGHT)  
                        .SetFontSize(14);
                    document.Add(table);
                    document.Add(new Paragraph("\n"));
                    document.Add(companyName);
                } 
                byte[] pdfBytes = stream.ToArray();
                if (pdfBytes.Length == 0) { return BadRequest("PDF generation failed, empty file."); }
                return File(pdfBytes, "application/pdf", $"PF_Details_{id}.pdf");
            }
        }
    }
}

