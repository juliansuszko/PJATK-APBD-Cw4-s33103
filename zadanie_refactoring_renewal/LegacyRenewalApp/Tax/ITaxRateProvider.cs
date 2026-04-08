namespace LegacyRenewalApp.Tax;

public interface ITaxRateProvider
{
    decimal GetTaxRateForCountry(string country);
}