using System;
using System.Collections.Generic;

namespace LegacyRenewalApp.Tax;

public class TaxRateProvider : ITaxRateProvider
{
    private readonly Dictionary<String, Decimal> taxRates = new()
    {
        { "Poland", 0.23m },
        { "Germany", 0.19m },
        { "Czech Republic", 0.21m },
        { "Norway", 0.25m },
    };
    
    public decimal GetTaxRateForCountry(string country)
    {
        if (taxRates.ContainsKey(country))
        {
            return taxRates[country];
        }
        
        throw new KeyNotFoundException($"Country {country} not found");
    }
}