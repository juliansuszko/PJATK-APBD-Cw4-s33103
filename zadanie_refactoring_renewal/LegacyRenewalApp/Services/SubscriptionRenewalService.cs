using System;
using LegacyRenewalApp.Discounts;
using LegacyRenewalApp.Fees;
using LegacyRenewalApp.Gateways;
using LegacyRenewalApp.Models;
using LegacyRenewalApp.Repositories;
using LegacyRenewalApp.Tax;
using LegacyRenewalApp.Validation;

namespace LegacyRenewalApp.Services
{
    public class SubscriptionRenewalService
    {
        private readonly ITaxCalculator _taxCalculator;
        private readonly IBillingGateway _billingGateway;
        
        private readonly ICustomerRepository _customerRepository;
        private readonly ISubscriptionPlanRepository _planRepository;
        
        private readonly ISubscriptionValidator _subscriptionValidator;
        
        private readonly IFeeCalculator _feeCalculator;
        
        private readonly IDiscountCalculator _discountCalculator;

        public SubscriptionRenewalService() : this(new TaxCalculator(new TaxRateProvider()),
            new BillingGatewayWrapper(),
            new CustomerRepository(),
            new SubscriptionPlanRepository(),
            new SubscriptionValidator(),
            new FeeCalculator(),
            new DiscountCalculator()
            )
        {
            
        }

        public SubscriptionRenewalService(ITaxCalculator taxCalculator,
            IBillingGateway billingGateway,
            ICustomerRepository customerRepository,
            ISubscriptionPlanRepository planRepository,
            ISubscriptionValidator subscriptionValidator,
            IFeeCalculator feeCalculator,
            IDiscountCalculator  discountCalculator
            )
        {
            _taxCalculator = taxCalculator;
            _billingGateway =  billingGateway;
            _customerRepository = customerRepository;
            _planRepository = planRepository;
            _subscriptionValidator = subscriptionValidator;
            _feeCalculator = feeCalculator;
            _discountCalculator =  discountCalculator;
        }

        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            _subscriptionValidator.Validate(customerId, planCode, seatCount, paymentMethod);

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();


            var customer = _customerRepository.GetById(customerId);
            var plan = _planRepository.GetByCode(normalizedPlanCode);
            

            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
            }

            decimal baseAmount = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
            decimal discountAmount = 0m;
            string notes = string.Empty;

            var discountResult = _discountCalculator.CalculateTotalDiscount(customer, plan, seatCount, baseAmount, useLoyaltyPoints);
            
            discountAmount = discountResult.Amount;
            notes += discountResult.Note;
            
            decimal subtotalAfterDiscount = baseAmount - discountAmount;

            decimal supportFee = 0m;
            var feeSupportResult = _feeCalculator.CalculateSupportFee(normalizedPlanCode, includePremiumSupport);
            supportFee = feeSupportResult.Amount;
            notes += feeSupportResult.Note;

            decimal paymentFee = 0m;
            var feePaymentResult = _feeCalculator.CalculatePaymentFee(normalizedPaymentMethod, (subtotalAfterDiscount + supportFee));
            paymentFee = feePaymentResult.Amount;
            notes += feePaymentResult.Note;

            decimal taxBase = subtotalAfterDiscount + supportFee + paymentFee;
            var taxResult = _taxCalculator.CalculateTax(taxBase, customer.Country);
            notes += taxResult.AdditionalNotes;
            

            var invoice = new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{normalizedPlanCode}",
                CustomerName = customer.FullName,
                PlanCode = normalizedPlanCode,
                PaymentMethod = normalizedPaymentMethod,
                SeatCount = seatCount,
                BaseAmount = Math.Round(baseAmount, 2, MidpointRounding.AwayFromZero),
                DiscountAmount = Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero),
                SupportFee = Math.Round(supportFee, 2, MidpointRounding.AwayFromZero),
                PaymentFee = Math.Round(paymentFee, 2, MidpointRounding.AwayFromZero),
                
                TaxAmount = Math.Round(taxResult.TaxAmount, 2, MidpointRounding.AwayFromZero),
                FinalAmount = Math.Round(taxResult.FinalAmount, 2, MidpointRounding.AwayFromZero),
                
                Notes = notes.Trim(),
                GeneratedAt = DateTime.UtcNow
            };

            _billingGateway.SaveInvoice(invoice);

            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                string subject = "Subscription renewal invoice";
                string body =
                    $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} " +
                    $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

                _billingGateway.SendEmail(customer.Email, subject, body);
            }

            return invoice;
        }
    }
}
