using System.Text;
using System.Xml.Linq;
using PS_BusinessCard.IService;
using PS_BusinessCard.Models;

namespace PS_BusinessCard.Services
{
    public class XmlService : IXmlService
    {
        #region Public Methods

        public async Task<List<BusinessCard>> ImportXmlAsync(Stream xmlStream)
        {
            var businessCards = new List<BusinessCard>();

            // Load the XML from the stream
            var xDocument = await XDocument.LoadAsync(xmlStream, LoadOptions.None, default);

            // Loop through each "BusinessCard" element in the XML
            foreach (var element in xDocument.Descendants("BusinessCard"))
            {
                var businessCard = new BusinessCard
                {
                    Name = element.Element("Name")?.Value ?? "",
                    Gender = element.Element("Gender")?.Value??"",
                    Phone = element.Element("Phone")?.Value??"",
                    Address = element.Element("Address")?.Value,
                    Email = element.Element("Email")?.Value,
                    DateOfBirth = DateTime.Parse(element.Element("DateOfBirth")?.Value??""),
                };

                businessCards.Add(businessCard);
            }

            return businessCards;
        }
        public async Task<byte[]> ExportToXmlAsync(IEnumerable<BusinessCard> businessCards)
        {
            var xml = new XDocument(new XElement("BusinessCards",
                businessCards.Select(card => new XElement("BusinessCard",
                    new XElement("Name", card.Name),
                    new XElement("Gender", card.Gender),
                    new XElement("Phone", card.Phone),
                    new XElement("Address", card.Address),
                    new XElement("Email", card.Email),
                    new XElement("DateOfBirth", card.DateOfBirth.ToString("yyyy-MM-dd"))
                ))
            ));

            var xmlBytes = Encoding.UTF8.GetBytes(xml.ToString());

            return await Task.FromResult(xmlBytes);
        }
        #endregion Public Methods
    }
}
