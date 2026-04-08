using LegacyRenewalApp.Models;

namespace LegacyRenewalApp.Discounts;

public interface IDiscountCalculator
{
    DiscountResult CalculateTotalDiscount(
        Customer customer,
        SubscriptionPlan plan,
        int seatCount,
        decimal baseAmount,
        bool useLoyaltyPoints);
}