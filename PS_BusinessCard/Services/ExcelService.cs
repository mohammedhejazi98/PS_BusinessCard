using OfficeOpenXml;

using PS_BusinessCard.IService;
using PS_BusinessCard.Models;

namespace PS_BusinessCard.Services
{
    public class ExcelService : IExcelService
    {
        #region Public Methods

        public async Task<byte[]> GenerateExcelAsync(IEnumerable<BusinessCard> businessCards)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Create an in-memory Excel package
            using var package = new ExcelPackage();
            // Add a worksheet to the Excel workbook
            var worksheet = package.Workbook.Worksheets.Add("BusinessCards");

            // Add header row
            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "Gender";
            worksheet.Cells[1, 3].Value = "Date of Birth";
            worksheet.Cells[1, 4].Value = "Email";
            worksheet.Cells[1, 5].Value = "Phone";
            worksheet.Cells[1, 6].Value = "Address";
            worksheet.Cells[1, 7].Value = "Photo";

            // Populate rows with business card data
            int row = 2;
            foreach (var card in businessCards)
            {
                worksheet.Cells[row, 1].Value = card.Name;
                worksheet.Cells[row, 2].Value = card.Gender;
                worksheet.Cells[row, 3].Value = card.DateOfBirth.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 4].Value = card.Email;
                worksheet.Cells[row, 5].Value = card.Phone;
                worksheet.Cells[row, 6].Value = card.Address;
                worksheet.Cells[row, 7].Value = card.PhotoBase64;
                row++;
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Return the Excel file as a byte array
            return await Task.FromResult(await package.GetAsByteArrayAsync());
        }

        public async Task<List<BusinessCard>> ImportExcelAsync(Stream excelStream)
        {
            var businessCards = new List<BusinessCard>();

            using (var package = new ExcelPackage(excelStream))
            {
                // Get the first worksheet in the Excel file
                var worksheet = package.Workbook.Worksheets[0];

                // Loop through each row (starting from row 2 to skip headers)
                for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                {
                    var businessCard = new BusinessCard
                    {
                        Name = worksheet.Cells[row, 1].Text,
                        Gender = worksheet.Cells[row, 2].Text,
                        DateOfBirth = DateTime.Parse(worksheet.Cells[row, 3].Text),
                        Email = worksheet.Cells[row, 4].Text,
                        Phone = worksheet.Cells[row, 5].Text,
                        Address = worksheet.Cells[row, 6].Text,
                        PhotoBase64 = worksheet.Cells[row, 7].Text  // Optional: handle image here
                    };

                    businessCards.Add(businessCard);
                }
            }

            return await Task.FromResult(businessCards);
        }

        #endregion Public Methods
    }
}
