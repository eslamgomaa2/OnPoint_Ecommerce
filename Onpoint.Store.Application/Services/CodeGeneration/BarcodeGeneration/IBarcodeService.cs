namespace Onpoint.Store.Application.Services.CodeGeneration.BarcodeGeneration
{
    public interface IBarcodeService
    {
        string GenerateValue();
        byte[] GenerateImage(string value);
    }

}
