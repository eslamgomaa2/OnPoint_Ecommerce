namespace Onpoint.Store.Application.Services.CodeGeneration.SkuGeneration
{
    public interface ISkuGeneratorService
    {
        Task<string> GenerateUniqueSkuAsync(string productName, int categoryId);


    }
}