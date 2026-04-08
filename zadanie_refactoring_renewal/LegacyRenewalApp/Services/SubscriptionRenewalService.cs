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
        
        private readonly ICustomerRepository _customerRepository;
        private readonly ISubscriptionPlanRepository _planRepository;
        
        private readonly ISubscriptionValidator _subscriptionValidator;
        
        private readonly IFeeCalculator _feeCalculator;
        
        private readonly IDiscountCalculator _discountCalculator;
        
        private readonly IInvoiceProcessor _invoiceProcessor;

        public SubscriptionRenewalService() : this(new TaxCalculator(new TaxRateProvider()),
            new CustomerRepository(),
            new SubscriptionPlanRepository(),
            new SubscriptionValidator(),
            new FeeCalculator(),
            new DiscountCalculator(),
            new InvoiceProcessor(new BillingGatewayWrapper())
            )
        {
            
        }

        public SubscriptionRenewalService(ITaxCalculator taxCalculator,
            ICustomerRepository customerRepository,
            ISubscriptionPlanRepository planRepository,
            ISubscriptionValidator subscriptionValidator,
            IFeeCalculator feeCalculator,
            IDiscountCalculator  discountCalculator,
            IInvoiceProcessor  invoiceProcessor
            )
        {
            _taxCalculator = taxCalculator;
            _customerRepository = customerRepository;
            _planRepository = planRepository;
            _subscriptionValidator = subscriptionValidator;
            _feeCalculator = feeCalculator;
            _discountCalculator =  discountCalculator;
            _invoiceProcessor = invoiceProcessor;
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
            string notes = string.Empty;

            var discountResult = _discountCalculator.CalculateTotalDiscount(customer, plan, seatCount, baseAmount, useLoyaltyPoints);
            
            var discountAmount = discountResult.Amount;
            notes += discountResult.Note;
            
            decimal subtotalAfterDiscount = baseAmount - discountAmount;

            var feeSupportResult = _feeCalculator.CalculateSupportFee(normalizedPlanCode, includePremiumSupport);
            var supportFee = feeSupportResult.Amount;
            notes += feeSupportResult.Note;

            var feePaymentResult = _feeCalculator.CalculatePaymentFee(normalizedPaymentMethod, (subtotalAfterDiscount + supportFee));
            var paymentFee = feePaymentResult.Amount;
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
            
            _invoiceProcessor.ProcessInvoice(invoice, customer);

            return invoice;
        }
    }
}
