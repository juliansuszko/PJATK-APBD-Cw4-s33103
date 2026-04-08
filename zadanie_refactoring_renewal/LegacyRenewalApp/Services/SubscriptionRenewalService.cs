using System;
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

        public SubscriptionRenewalService() : this(new TaxCalculator(new TaxRateProvider()),
            new BillingGatewayWrapper(),
            new CustomerRepository(),
            new SubscriptionPlanRepository(),
            new SubscriptionValidator(),
            new FeeCalculator()
            )
        {
            
        }

        public SubscriptionRenewalService(ITaxCalculator taxCalculator,
            IBillingGateway billingGateway,
            ICustomerRepository customerRepository,
            ISubscriptionPlanRepository planRepository,
            ISubscriptionValidator subscriptionValidator,
            IFeeCalculator feeCalculator
            )
        {
            _taxCalculator = taxCalculator;
            _billingGateway =  billingGateway;
            _customerRepository = customerRepository;
            _planRepository = planRepository;
            _subscriptionValidator = subscriptionValidator;
            _feeCalculator = feeCalculator;
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

            if (customer.Segment == "Silver")
            {
                discountAmount += baseAmount * 0.05m;
                notes += "silver discount; ";
            }
            else if (customer.Segment == "Gold")
            {
                discountAmount += baseAmount * 0.10m;
                notes += "gold discount; ";
            }
            else if (customer.Segment == "Platinum")
            {
                discountAmount += baseAmount * 0.15m;
                notes += "platinum discount; ";
            }
            else if (customer.Segment == "Education" && plan.IsEducationEligible)
            {
                discountAmount += baseAmount * 0.20m;
                notes += "education discount; ";
            }

            if (customer.YearsWithCompany >= 5)
            {
                discountAmount += baseAmount * 0.07m;
                notes += "long-term loyalty discount; ";
            }
            else if (customer.YearsWithCompany >= 2)
            {
                discountAmount += baseAmount * 0.03m;
                notes += "basic loyalty discount; ";
            }

            if (seatCount >= 50)
            {
                discountAmount += baseAmount * 0.12m;
                notes += "large team discount; ";
            }
            else if (seatCount >= 20)
            {
                discountAmount += baseAmount * 0.08m;
                notes += "medium team discount; ";
            }
            else if (seatCount >= 10)
            {
                discountAmount += baseAmount * 0.04m;
                notes += "small team discount; ";
            }

            if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
            {
                int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
                discountAmount += pointsToUse;
                notes += $"loyalty points used: {pointsToUse}; ";
            }

            decimal subtotalAfterDiscount = baseAmount - discountAmount;
            if (subtotalAfterDiscount < 300m)
            {
                subtotalAfterDiscount = 300m;
                notes += "minimum discounted subtotal applied; ";
            }

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
