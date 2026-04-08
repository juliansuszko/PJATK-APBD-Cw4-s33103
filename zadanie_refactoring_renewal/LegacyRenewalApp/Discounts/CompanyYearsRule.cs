using LegacyRenewalApp.Models;

namespace LegacyRenewalApp.Discounts;

public class CompanyYearsRule : IDiscountRule
{
    public DiscountResult calculateDiscount(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount)
    {
        decimal rate = 0m;
        string note = string.Empty;
        
        if (customer.YearsWithCompany >= 5)
        {
            rate = 0.07m;
            note = "long-term loyalty discount; ";
        }
        else if (customer.YearsWithCompany >= 2)
        {
            rate = 0.03m;
            note = "basic loyalty discount; ";
        }

        if (rate == 0) return new DiscountResult(0, string.Empty);

        return new DiscountResult(rate * baseAmount, note);
    }
}