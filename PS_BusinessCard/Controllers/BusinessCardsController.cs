using Microsoft.AspNetCore.Mvc;

using PS_BusinessCard.Dtos;
using PS_BusinessCard.Helper;
using PS_BusinessCard.IService;
using PS_BusinessCard.Models;
using PS_BusinessCard.Repositories;

using System.Drawing.Imaging;

using ZXing;
using ZXing.Windows.Compatibility;

namespace PS_BusinessCard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessCardsController(
        IBusinessCardRepository repository,
        IExcelService excelService,
        IXmlService xmlService,
        IQrCodeService qrCodeService,
        ILogger<BusinessCardsController> logger) : ControllerBase
    {
        #region Public Methods

        /// <summary>
        /// Adds a new business card. Supports file upload for QR code images, Excel, or XML files.
        /// </summary>
        /// <param name="businessCard">DTO containing business card details and file upload.</param>
        /// <returns>ActionResult indicating the result of the operation.</returns>
        [HttpPost("AddBusinessCard")]
        public async Task<ActionResult> AddBusinessCard([FromForm] CreateBusinessCardDto businessCard)
        {
            logger.LogInformation("AddBusinessCard called.");

            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid model state: {ModelStateErrors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            if (businessCard.FileUpload is not null)
            {
                logger.LogInformation("File uploaded: {FileName}", businessCard.FileUpload.FileName);

                var extension = Path.GetExtension(businessCard.FileUpload.FileName).ToLower();
                string[] allowedExtensions = [".xlsx", ".xls", ".xml", ".png", ".jpg", ".jpeg"];

                bool isAllowedExtension = allowedExtensions.Contains(extension);
                if (isAllowedExtension)
                {
                    if (ImageHelper.IsImage(businessCard.FileUpload))
                    {
                        logger.LogInformation("Processing QR image file.");

                        await using var stream = businessCard.FileUpload.OpenReadStream();
                        var result = qrCodeService.DecodeQrCode(stream);

                        if (result != null)
                        {
                            logger.LogInformation("QR code decoded successfully.");
                            result.Id = 0;
                            await repository.Add(result);
                            return Ok(new { Message = "Business cards imported successfully." });
                        }

                        logger.LogWarning("QR code decoding failed. Invalid or corrupt QR code uploaded.");
                        return BadRequest("Failed to decode QR code. Please ensure the image contains a valid QR code.");
                    }

                    if (extension is ".xlsx" or ".xls")
                    {
                        logger.LogInformation("Processing Excel file: {FileName}", businessCard.FileUpload.FileName);

                        await using var stream = businessCard.FileUpload.OpenReadStream();
                        try
                        {
                            var businessCards = await excelService.ImportExcelAsync(stream);
                            foreach (var card in businessCards)
                            {
                                await repository.Add(card);
                            }

                            logger.LogInformation("{Count} business cards imported from Excel file.", businessCards.Count);
                            return Ok(new { Message = $"{businessCards.Count} business cards imported successfully." });
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "An error occurred while processing the Excel file: {FileName}", businessCard.FileUpload.FileName);
                            return BadRequest($"An error occurred while processing the Excel file: {ex.Message}");
                        }
                    }

                    if (extension == ".xml")
                    {
                        logger.LogInformation("Processing XML file: {FileName}", businessCard.FileUpload.FileName);

                        await using var stream = businessCard.FileUpload.OpenReadStream();
                        List<BusinessCard> businessCards = await xmlService.ImportXmlAsync(stream);

                        foreach (var card in businessCards)
                        {
                            await repository.Add(card);
                        }

                        logger.LogInformation("{Count} business cards imported from XML file.", businessCards.Count);
                        return Ok(new { Message = $"{businessCards.Count} business cards imported successfully." });
                    }
                }

                logger.LogWarning("Invalid file type uploaded: {FileName}", businessCard.FileUpload.FileName);
                ModelState.AddModelError("FileUpload", "Only QR image and (xls, xlsx, xml) files are allowed.");
                return BadRequest(ModelState);
            }

            logger.LogInformation("Adding business card without file upload.");
            await repository.Add(new BusinessCard
            {
                Phone = businessCard.Phone,
                Gender = businessCard.Gender,
                Name = businessCard.Name,
                Address = businessCard.Address,
                DateOfBirth = businessCard.DateOfBirth,
                Email = businessCard.Email,
                PhotoBase64 = businessCard.PhotoBase64
            });

            logger.LogInformation("Business card added successfully: {Name}", businessCard.Name);
            return CreatedAtAction(nameof(GetBusinessCard), new { id = businessCard.Id }, businessCard);
        }

        /// <summary>
        /// Deletes a business card by ID.
        /// </summary>
        /// <param name="id">ID of the business card to delete.</param>
        /// <returns>ActionResult indicating the result of the delete operation.</returns>
        [HttpDelete("DeleteBusinessCard")]
        public async Task<ActionResult> DeleteBusinessCard(int id)
        {
            logger.LogInformation("Deleting business card with ID: {Id}", id);
            await repository.Delete(id);
            logger.LogInformation("Business card deleted successfully.");
            return NoContent();
        }

        /// <summary>
        /// Exports all business cards to an Excel file.
        /// </summary>
        /// <returns>An Excel file containing all business cards.</returns>
        [HttpGet("ExportToExcel")]
        public async Task<IActionResult> ExportToExcel()
        {
            logger.LogInformation("Exporting business cards to Excel.");
            var businessCards = await repository.GetAll();
            var excelFile = await excelService.GenerateExcelAsync(businessCards);

            logger.LogInformation("{Count} business cards exported to Excel.", businessCards.Count());
            return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BusinessCards.xlsx");
        }

        /// <summary>
        /// Exports all business cards to an XML file.
        /// </summary>
        /// <returns>An XML file containing all business cards.</returns>
        [HttpGet("ExportToXml")]
        public async Task<IActionResult> ExportToXml()
        {
            logger.LogInformation("Exporting business cards to XML.");
            var businessCards = await repository.GetAll();
            var xmlFile = await xmlService.ExportToXmlAsync(businessCards);

            logger.LogInformation("{Count} business cards exported to XML.", businessCards.Count());
            return File(xmlFile, "application/xml", "BusinessCards.xml");
        }

        /// <summary>
        /// Generates a QR code for a business card by ID.
        /// </summary>
        /// <param name="id">ID of the business card for which to generate a QR code.</param>
        /// <returns>A PNG file containing the generated QR code.</returns>
        [HttpGet("GenerateQr")]
        public async Task<ActionResult> GenerateQr(int id)
        {
            logger.LogInformation("Generating QR code for business card with ID: {Id}", id);
            var card = await repository.GetById(id);
            if (card is null)
            {
                logger.LogWarning("Business card not found with ID: {Id}", id);
                return NotFound("BusinessCard Not Found");
            }
            var cardToBeGenerated = new BusinessCard
            {
                Gender = card.Gender,
                Phone = card.Phone,
                Name = card.Name,
                Id = card.Id,
                Email = card.Email,
                Address = card.Address,
                DateOfBirth = card.DateOfBirth
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(cardToBeGenerated);
            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options =
                {
                    Width = 300,
                    Height = 300
                }
            };

            try
            {
                using var memoryStream = new MemoryStream();
                var barcodeBitmap = barcodeWriter.Write(json);
                barcodeBitmap.Save(memoryStream, ImageFormat.Png);

                logger.LogInformation("QR code generated successfully for business card: {Name}", card.Name);
                return File(memoryStream.ToArray(), "image/png", $"{card.Name}-Qr.png");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while generating the QR code for business card: {Id}", id);
                return StatusCode(500, "An error occurred while generating the QR code.");
            }
        }

        /// <summary>
        /// Retrieves a business card by ID.
        /// </summary>
        /// <param name="id">ID of the business card to retrieve.</param>
        /// <returns>The requested business card.</returns>
        [HttpGet("GetBusinessCard")]
        public async Task<ActionResult<BusinessCard>> GetBusinessCard(int id)
        {
            logger.LogInformation("Fetching business card with ID: {Id}", id);
            var card = await repository.GetById(id);
            if (card == null)
            {
                logger.LogWarning("Business card not found with ID: {Id}", id);
                return NotFound("Business Card Not Found");
            }

            logger.LogInformation("Business card fetched successfully.");
            return Ok(card);
        }

        /// <summary>
        /// Retrieves all business cards.
        /// </summary>
        /// <returns>A list of all business cards.</returns>
        [HttpGet("GetBusinessCards")]
        public async Task<ActionResult<IEnumerable<BusinessCard>>> GetBusinessCards()
        {
            logger.LogInformation("Fetching all business cards.");
            return Ok(await repository.GetAll());
        }

        /// <summary>
        /// Updates an existing business card.
        /// </summary>
        /// <param name="id">ID of the business card to update.</param>
        /// <param name="businessCard">The updated business card details.</param>
        /// <returns>ActionResult indicating the result of the update operation.</returns>
        [HttpPut("UpdateBusinessCard")]
        public async Task<ActionResult> UpdateBusinessCard(int id, BusinessCard businessCard)
        {
            if (id != businessCard.Id)
            {
                logger.LogWarning("Business card ID mismatch: {Id}", id);
                return BadRequest();
            }

            try
            {
                logger.LogInformation("Updating business card with ID: {Id}", id);
                await repository.Update(businessCard);
                logger.LogInformation("Business card updated successfully.");
                return CreatedAtAction(nameof(GetBusinessCard), new { id = businessCard.Id }, businessCard);
            }
            catch (Exception e)
            {
                logger.LogError(e, "An error occurred while updating the business card with ID: {Id}", id);
                return StatusCode(500, "An error occurred while updating the business card.");
            }
        }

        #endregion Public Methods
    }
}
