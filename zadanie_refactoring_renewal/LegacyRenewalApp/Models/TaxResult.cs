namespace LegacyRenewalApp.Models;

public record TaxResult(decimal TaxBase, decimal TaxAmount, decimal FinalAmount, string AdditionalNotes);