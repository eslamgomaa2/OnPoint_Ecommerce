using Onpoint.Store.Application.DTOs.Product;

public class CreateDiscountDtoListValidator
{
    public static bool HasOverlap(List<CreateDiscountDto> discounts)
    {
        var sorted = discounts.OrderBy(d => d.StartDate).ToList();

        for (int i = 0; i < sorted.Count - 1; i++)
        {
            if (sorted[i].EndDate >= sorted[i + 1].StartDate)
                return true;
        }

        return false;
    }
}