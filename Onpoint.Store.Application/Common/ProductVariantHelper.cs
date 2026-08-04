using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Common
{
    public static class ProductVariantHelper
    {
        /// <summary>
        /// Calculates total available quantity for stocks, optionally filtered by branch.
        /// </summary>
        public static int CalculateAvailableQuantity(IEnumerable<Stock>? stocks, int? branchId = null)
        {
            if (stocks == null) return 0;
            var query = stocks.AsEnumerable();
            if (branchId.HasValue)
                query = query.Where(s => s.BranchId == branchId.Value);

            return query.Sum(s => Math.Max(0, s.Quantity - s.ReservedQuantity));
        }

        /// <summary>
        /// Checks if available stock quantity is greater than zero.
        /// </summary>
        public static bool IsInStock(IEnumerable<Stock>? stocks, int? branchId = null)
        {
            return CalculateAvailableQuantity(stocks, branchId) > 0;
        }

        /// <summary>
        /// Selects the default variant for a product following business priority:
        /// 1. Only active non-deleted variants.
        /// 2. Prioritize variants that are in stock (lowest price, then lowest ID).
        /// 3. Fallback to lowest price active variant if all are out of stock.
        /// 4. Returns null if no active variants exist.
        /// </summary>
        public static ProductVariant? SelectDefaultVariant(IEnumerable<ProductVariant>? variants, int? branchId = null)
        {
            if (variants == null) return null;

            var activeVariants = variants
                .Where(v => v.IsActive && !v.IsDeleted)
                .ToList();

            if (!activeVariants.Any()) return null;

            // 1. In-Stock active variants
            var inStockVariant = activeVariants
                .Where(v => IsInStock(v.Stocks, branchId))
                .OrderBy(v => v.Price)
                .ThenBy(v => v.Id)
                .FirstOrDefault();

            if (inStockVariant != null)
                return inStockVariant;

            // 2. Fallback: Lowest price active variant (deterministic)
            return activeVariants
                .OrderBy(v => v.Price)
                .ThenBy(v => v.Id)
                .FirstOrDefault();
        }
    }
}
