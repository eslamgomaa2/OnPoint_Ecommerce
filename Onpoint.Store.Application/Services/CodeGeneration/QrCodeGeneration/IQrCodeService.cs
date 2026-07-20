namespace Onpoint.Store.Application.Services.CodeGeneration.QrCodeGeneration
{

    public interface IQrCodeService
    {
        string GenerateValue(string sku);
        byte[] GenerateImage(string value);
    }
}
