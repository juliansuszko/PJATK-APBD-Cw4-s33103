using System;
using System.Collections.Generic;
using LegacyRenewalApp.Models;

namespace LegacyRenewalApp.Discounts;

public class CustomerRule : IDiscountRule
{
    
    private record SegmentAttributes(decimal SegmentAmount, string Note);

    private static readonly Dictionary<string, SegmentAttributes> SegmentRates = new()
    {
        { "Silver", new SegmentAttributes(0.05m, "silver discount; ") },
        { "Gold", new SegmentAttributes(0.10m, "gold discount; ") },
        { "Platinum", new SegmentAttributes(0.15m, "platinum discount; ") },
        { "Education", new SegmentAttributes(0.20m, "education discount; ") },
    };
    
    public DiscountResult calculateDiscount(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount)
    {
        if (SegmentRates.ContainsKey(customer.Segment))
        {
            if (customer.Segment == "Education")
            {
                if (plan.IsEducationEligible)
                {
                    return new DiscountResult(SegmentRates["Education"].SegmentAmount * baseAmount, SegmentRates["Education"].Note);
                }
                else
                {
                    return new DiscountResult(0, string.Empty);
                }
            }

            return new DiscountResult(SegmentRates[customer.Segment].SegmentAmount * baseAmount, SegmentRates[customer.Segment].Note);
        }

        return new DiscountResult(0, string.Empty);
    }
}