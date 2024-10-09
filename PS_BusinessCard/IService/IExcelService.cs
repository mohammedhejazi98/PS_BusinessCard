using PS_BusinessCard.Models;

namespace PS_BusinessCard.IService;

public interface IExcelService
{
    #region Public Methods

    Task<byte[]> GenerateExcelAsync(IEnumerable<BusinessCard> businessCards);
    Task<List<BusinessCard>> ImportExcelAsync(Stream excelStream);

    #endregion Public Methods

}