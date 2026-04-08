using LegacyRenewalApp.Models;

namespace LegacyRenewalApp.Services;

public interface IInvoiceProcessor
{
    void ProcessInvoice(RenewalInvoice invoice, Customer customer);
}