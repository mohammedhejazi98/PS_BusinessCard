using PS_BusinessCard.Models;

namespace PS_BusinessCard.IService;

public interface IXmlService
{
    #region Public Methods

    Task<List<BusinessCard>> ImportXmlAsync(Stream xmlStream);
    Task<byte[]> ExportToXmlAsync(IEnumerable<BusinessCard> businessCards);

    #endregion Public Methods

}