using ZXing;
using ZXing.QrCode;
using System.Drawing;
using System.Drawing.Imaging;

namespace SabiMarket.Infrastructure.Helpers
{
    public static class QRCodeHelper
    {
        public static string GenerateQRCode(string data, int width = 200, int height = 200)
        {
            try
            {
                var writer = new BarcodeWriterPixelData
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions
                    {
                        Height = height,
                        Width = width,
                        Margin = 1,
                        ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.H
                    }
                };

                var pixelData = writer.Write(data);

                using var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb);
                using var ms = new MemoryStream();

                var bitmapData = bitmap.LockBits(
                    new Rectangle(0, 0, pixelData.Width, pixelData.Height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppRgb);

                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(
                        pixelData.Pixels,
                        0,
                        bitmapData.Scan0,
                        pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }

                bitmap.Save(ms, ImageFormat.Png);
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating QR code", ex);
            }
        }
    }
}