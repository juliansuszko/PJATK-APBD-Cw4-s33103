namespace LegacyRenewalApp;

public interface ITaxCalculator
{
    TaxResult CalculateTax(decimal taxBase, string country);
}