using LegacyRenewalApp.Models;

namespace LegacyRenewalApp.Discounts;

public interface IDiscountRule
{
    DiscountResult calculateDiscount(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount);
}