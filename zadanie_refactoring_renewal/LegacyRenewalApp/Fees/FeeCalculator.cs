using System;
using System.Collections.Generic;
using LegacyRenewalApp.Models;

namespace LegacyRenewalApp.Fees;

public class FeeCalculator : IFeeCalculator
{
    
    private static readonly Dictionary<string, decimal> SupportFees = new()
    {
        {"START", 250m },
        {"PRO", 400m },
        {"ENTERPRISE", 700m}
    };
    
    private record PaymentInput(decimal Rate, string Note);

    private static readonly Dictionary<string, PaymentInput> PaymentFees = new()
    {
        { "CARD", new PaymentInput(0.02m, "card payment fee; ") },
        { "BANK_TRANSFER", new PaymentInput(0.01m, "bank transfer fee; ") },
        { "PAYPAL", new PaymentInput(0.035m, "paypal fee; ") },
        { "INVOICE", new PaymentInput(0m, "invoice payment; ") }
    };
    
    
    public FeeResult CalculateSupportFee(string planCode, bool includePremiumSupport)
    {
        if (!includePremiumSupport || !SupportFees.ContainsKey(planCode)) return new FeeResult(0m, string.Empty);
        return new FeeResult(SupportFees[planCode], "premium support included; ");
    }

    public FeeResult CalculatePaymentFee(string paymentMethod, decimal amountBeforeFee)
    {
        if (!PaymentFees.ContainsKey(paymentMethod))
        {
            throw new ArgumentException("Unsupported payment method");
        }
        decimal feeAmount = amountBeforeFee * PaymentFees[paymentMethod].Rate;
        
        return new FeeResult(feeAmount, PaymentFees[paymentMethod].Note);
    }
}