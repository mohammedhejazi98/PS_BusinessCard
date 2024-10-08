using Microsoft.AspNetCore.Mvc;

using PS_BusinessCard.Dtos;
using PS_BusinessCard.Models;
using PS_BusinessCard.Repositories;

using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Xml.Linq;

using ZXing;
using ZXing.Windows.Compatibility;

namespace PS_BusinessCard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessCardsController(IBusinessCardRepository repository) : ControllerBase
    {
        #region Public Methods

        [HttpPost]
        public async Task<ActionResult> AddBusinessCard([FromForm] CreateBusinessCardDto businessCard)
        {
            if (businessCard.FileUpload is not null )
            {
                if (IsImage(businessCard.FileUpload))
                {
                    var bitmap = new Bitmap(businessCard.FileUpload.FileName);

                    // Decoding the barcode
                    var reader = new BarcodeReader();
                    var result = reader.Decode(bitmap);

                    // Printing the result
                    if (result != null)
                    {
                        var card = Newtonsoft.Json.JsonConvert.DeserializeObject<BusinessCard>(result.Text);
                        Console.WriteLine("QR code content: " + result.Text);
                    }
                    else
                    {
                        Console.WriteLine("QR code could not be decoded");
                    }

                }
                else
                {
                    ModelState.AddModelError("FileUpload", "Only image files are allowed.");
                    return BadRequest(ModelState); // Return bad request with validation error

                }
            }
         
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
            return CreatedAtAction(nameof(GetBusinessCard), new { id = businessCard.Id }, businessCard);
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBusinessCard(int id)
        {
            await repository.Delete(id);
            return NoContent();
        }
        
        [HttpPost("GenerateQr")]
        public async Task<ActionResult> GenerateQr(int id)
        {
            var card = await repository.GetById(id);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(card);

            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options =
                {
                    Width = 200,
                    Height = 200
                }
            };

            using var memoryStream = new MemoryStream();
            var barcodeBitmap = barcodeWriter.Write(json);
            barcodeBitmap.Save(memoryStream, ImageFormat.Png);
            return File(memoryStream.ToArray(), "image/png", $"{card.Name}.png");
        }

        [HttpGet("ExportToXml")]
        public async Task<IActionResult> ExportToXml()
        {
            var cards = await repository.GetAll();
            var xml = new XDocument(new XElement("BusinessCards",
                cards.Select(card => new XElement("BusinessCard",
                    new XElement("Id", card.Id),
                    new XElement("Name", card.Name),
                    new XElement("Gender", card.Gender),
                    new XElement("Phone", card.Phone),
                    new XElement("Address", card.Address),
                    new XElement("Email", card.Email),
                    new XElement("DateOfBirth", card.DateOfBirth),
                    new XElement("PhotoBase64", card.PhotoBase64)
                ))));

            return File(Encoding.UTF8.GetBytes(xml.ToString()), "application/xml", "BusinessCards.xml");
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<BusinessCard>> GetBusinessCard(int id)
        {
          
            var card = await repository.GetById(id);
            if (card == null) return NotFound();
            return Ok(card);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BusinessCard>>> GetBusinessCards()
        {
            return Ok(await repository.GetAll());
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateBusinessCard(int id, BusinessCard businessCard)
        {
            if (id != businessCard.Id) return BadRequest();
            await repository.Update(businessCard);
            return NoContent();
        }

        #endregion Public Methods

        public static bool IsImage(IFormFile file)
        {
            if (file.Length < 8) // Check for minimum file size
            {
                return false;
            }

            byte[] header = new byte[8];
            using (var stream = file.OpenReadStream())
            {
                stream.Read(header, 0, 8);
            }

            // Check magic bytes for common image formats
            return
                header.SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }) // JPEG
                || header.SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }) // PNG
                || header.SequenceEqual(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }); // GIF
            // Add checks for other image formats as needed
        }

    }


}
