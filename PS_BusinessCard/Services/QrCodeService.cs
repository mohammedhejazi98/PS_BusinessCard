using PS_BusinessCard.Models;

using System.Drawing;
using PS_BusinessCard.IService;
using ZXing.Windows.Compatibility;

namespace PS_BusinessCard.Services
{
    public class QrCodeService : IQrCodeService
    {
        #region Public Methods

        public BusinessCard? DecodeQrCode(Stream stream)
        {
            var bitmap = new Bitmap(stream);
            var reader = new BarcodeReader();
            var result = reader.Decode(bitmap);

            if (result != null)
                return Newtonsoft.Json.JsonConvert.DeserializeObject<BusinessCard>(result.Text);

            return null;
        }

        #endregion Public Methods
    }
}
