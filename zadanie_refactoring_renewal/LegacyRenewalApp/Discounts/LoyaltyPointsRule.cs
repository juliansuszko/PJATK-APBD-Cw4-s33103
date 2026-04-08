using LegacyRenewalApp.Models;

namespace LegacyRenewalApp.Discounts;

public class LoyaltyPointsRule : IDiscountRule
{
    public DiscountResult calculateDiscount(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount)
    {
        if (customer.LoyaltyPoints <= 0)
        {
            return new DiscountResult(0, string.Empty);
        }
        
        int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
        
        return new DiscountResult(pointsToUse, $"loyalty points used: {pointsToUse}; ");
    }
}