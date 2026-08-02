

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ZXing;
using ZXing.Common;

namespace Onpoint.Store.Application.Services.CodeGeneration.BarcodeGeneration
{
    namespace Onpoint.Store.Application.Services.CodeGeneration
    {

        public class BarcodeService : IBarcodeService
        {
            public string GenerateValue()
            {
                return Guid.NewGuid().ToString("N")[..12].ToUpper();
            }
            public byte[] GenerateImage(string value)
            {
                var writer = new BarcodeWriterPixelData
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions { Width = 300, Height = 100, Margin = 10 }
                };

                var pixelData = writer.Write(value);

                using Image<Bgra32> image = Image.LoadPixelData<Bgra32>(
                    pixelData.Pixels,
                    pixelData.Width,
                    pixelData.Height);

                using var ms = new MemoryStream();
                image.SaveAsPng(ms);
                return ms.ToArray();
            }
        }
    }
}
