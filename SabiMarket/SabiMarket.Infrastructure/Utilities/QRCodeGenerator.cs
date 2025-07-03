/*using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using QRCoder;

namespace SabiMarket.Infrastructure.Utilities
{
    public static class QRCodeGeneratorService
    {
        public static byte[] GenerateQRCode<T>(T data)
        {
            byte[] qrCodeImage;
            // Serialize the data into JSON format
            var qrData = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true // Makes the JSON readable
            });

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                qrCodeImage = qrCode.GetGraphic(20);
            }

            return qrCodeImage;
        }
    }
}
*/