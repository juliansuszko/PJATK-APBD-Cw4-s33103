namespace LegacyRenewalApp.Validation;

public interface ISubscriptionValidator
{
    void Validate(int customerId, string planCode, int seatCount, string paymentMethod);
}