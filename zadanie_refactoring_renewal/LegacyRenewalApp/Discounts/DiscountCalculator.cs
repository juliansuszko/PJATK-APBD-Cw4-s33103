using System.Collections.Generic;
using LegacyRenewalApp.Models;

namespace LegacyRenewalApp.Discounts;

public class DiscountCalculator : IDiscountCalculator
{
    public DiscountResult CalculateTotalDiscount(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount,
        bool useLoyaltyPoints)
    {
        var rules = new List<IDiscountRule>
        {
            new CustomerRule(),
            new CompanyYearsRule(),
            new SeatRule()
        };

        if (useLoyaltyPoints)
        {
            rules.Add(new LoyaltyPointsRule());
        }

        decimal totalDiscount = 0;
        string finalNotes = string.Empty;

        foreach (var rule in rules)
        {
            var result = rule.calculateDiscount(customer, plan, seatCount, baseAmount);
            totalDiscount += result.Amount;
            finalNotes += result.Note;
        }

        decimal subtotalAfterDiscount = baseAmount - totalDiscount;
        if (subtotalAfterDiscount < 300m)
        {
            subtotalAfterDiscount = 300m;
            finalNotes += "minimum discounted subtotal applied; ";
        }
        
        return new DiscountResult(totalDiscount, finalNotes);

    }
}