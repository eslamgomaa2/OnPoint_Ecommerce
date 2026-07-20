using Onpoint.Store.Application.Services.CodeGeneration.QrCodeGeneration;
using QRCoder;

namespace Onpoint.Store.Application.Services.CodeGeneration
{

    public class QrCodeService : IQrCodeService
    {
        public string GenerateValue(string sku) => $"PRODUCT-SKU:{sku}";

        public byte[] GenerateImage(string value)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(value, QRCodeGenerator.ECCLevel.Q);
            var pngQr = new PngByteQRCode(data);
            return pngQr.GetGraphic(20);
        }
    }
}