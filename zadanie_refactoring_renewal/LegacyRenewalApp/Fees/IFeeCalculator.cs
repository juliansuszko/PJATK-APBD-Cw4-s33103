using LegacyRenewalApp.Models;

namespace LegacyRenewalApp.Fees;

public interface IFeeCalculator
{
    FeeResult CalculateSupportFee(string planCode, bool includePremiumSupport);
    FeeResult CalculatePaymentFee(string paymentMethod, decimal amountBeforeFee);
}