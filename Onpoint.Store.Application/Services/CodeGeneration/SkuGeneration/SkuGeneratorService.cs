using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.CodeGeneration.SkuGeneration
{
    public class SkuGeneratorService : ISkuGeneratorService
    {
        private readonly IUnitOfWork _unitOfWork;
        public SkuGeneratorService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<string> GenerateUniqueSkuAsync(string productName, int categoryId)
        {
            var prefix = BuildPrefix(productName, categoryId);
            string candidate;
            var attempts = 0;

            do
            {
                var suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
                candidate = $"{prefix}-{suffix}";
                attempts++;
            }
            while (await _unitOfWork.Products.SkuExistsAsync(candidate) && attempts < 10);

            if (attempts >= 10)
                throw new InvalidOperationException("Could not generate a unique SKU after multiple attempts.");

            return candidate;
        }



        private static string BuildPrefix(string productName, int categoryId)
        {
            var letters = new string((productName ?? "PRD")
                .Where(char.IsLetterOrDigit)
                .Take(3)
                .ToArray())
                .ToUpperInvariant();

            if (letters.Length == 0) letters = "PRD";
            return $"{letters}{categoryId}";
        }
    }
}