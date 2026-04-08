using LegacyRenewalApp.Models;

namespace LegacyRenewalApp.Tax;

public class TaxCalculator : ITaxCalculator
{
    private readonly ITaxRateProvider _taxRateProvider;

    public TaxCalculator(ITaxRateProvider taxRateProvider)
    {
        _taxRateProvider = taxRateProvider;
    }

    public TaxResult CalculateTax(decimal taxBase, string country)
    {
        decimal taxRate = _taxRateProvider.GetTaxRateForCountry(country);
        decimal taxAmount = taxBase * taxRate;
        decimal finalAmount = taxBase + taxAmount;

        if (finalAmount < 500m)
        {
            finalAmount = 500m;
            string additionalNotes = "minimum invoice amount applied; ";
            return new TaxResult(taxBase, taxAmount, finalAmount, additionalNotes);
        }
        
        return new TaxResult(taxBase, taxRate, finalAmount, string.Empty);
    }
}