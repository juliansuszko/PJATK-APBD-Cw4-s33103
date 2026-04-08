using LegacyRenewalApp.Gateways;
using LegacyRenewalApp.Models;

namespace LegacyRenewalApp.Services;

public class InvoiceProcessor : IInvoiceProcessor
{
    private readonly IBillingGateway _billingGateway;

    public InvoiceProcessor(IBillingGateway billingGateway)
    {
        _billingGateway = billingGateway;
    }


    public void ProcessInvoice(RenewalInvoice invoice, Customer customer)
    {
        _billingGateway.SaveInvoice(invoice);

        if (!string.IsNullOrWhiteSpace(customer.Email))
        {
            string subject = "Subscription renewal invoice";
            string body =
                $"Hello {customer.FullName}, your renewal for plan {invoice.PlanCode} " +
                $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

            _billingGateway.SendEmail(customer.Email, subject, body);
        }
    }
}