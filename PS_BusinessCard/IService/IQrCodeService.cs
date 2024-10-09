using PS_BusinessCard.Models;

namespace PS_BusinessCard.IService;

public interface IQrCodeService
{
    #region Public Methods

    BusinessCard? DecodeQrCode(Stream stream);

    #endregion Public Methods
}