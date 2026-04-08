using LegacyRenewalApp.Models;

namespace LegacyRenewalApp.Discounts;

public class SeatRule : IDiscountRule
{
    public DiscountResult calculateDiscount(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount)
    {
        decimal rate = 0m;
        string note = string.Empty;
        
        if (seatCount >= 50)
        {
            rate = 0.12m;
            note = "large team discount; ";
        }
        else if (seatCount >= 20)
        {
            rate = 0.08m;
            note = "medium team discount; ";
        }
        else if (seatCount >= 10)
        {
            rate = 0.04m;
            note = "small team discount; ";
        }
        
        return new DiscountResult(rate * baseAmount, note);
    }
}