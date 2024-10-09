namespace PS_BusinessCard.Helper
{
    public static class ImageHelper
    {
        private static int _read;

        #region Public Methods

        public static bool IsImage(IFormFile file)
        {
            if (file.Length < 8)
            {
                return false;
            }

            byte[] header = new byte[8];
            using (var stream = file.OpenReadStream())
            {
                _read = stream.Read(header, 0, 8);
            }

            // Check magic bytes for common image formats
            return
                header.SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }) // JPEG
                || header.SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } // PNG
                );
        }

        #endregion Public Methods

    }
}
