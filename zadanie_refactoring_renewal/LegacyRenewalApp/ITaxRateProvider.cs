namespace LegacyRenewalApp;

public interface ITaxRateProvider
{
    decimal GetTaxRateForCountry(string country);
}