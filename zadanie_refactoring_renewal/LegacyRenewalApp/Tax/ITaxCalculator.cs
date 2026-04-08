using LegacyRenewalApp.Models;

namespace LegacyRenewalApp.Tax;

public interface ITaxCalculator
{
    TaxResult CalculateTax(decimal taxBase, string country);
}