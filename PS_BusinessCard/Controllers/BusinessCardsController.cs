using Microsoft.AspNetCore.Mvc;

using PS_BusinessCard.Models;
using PS_BusinessCard.Repositories;

using System.Text;
using System.Xml.Linq;

namespace PS_BusinessCard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessCardsController(IBusinessCardRepository repository) : ControllerBase
    {
        #region Public Methods

        [HttpPost]
        public async Task<ActionResult> AddBusinessCard(BusinessCard businessCard)
        {
            await repository.Add(businessCard);
            return CreatedAtAction(nameof(GetBusinessCard), new { id = businessCard.Id }, businessCard);
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBusinessCard(int id)
        {
            await repository.Delete(id);
            return NoContent();
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
    }
}
