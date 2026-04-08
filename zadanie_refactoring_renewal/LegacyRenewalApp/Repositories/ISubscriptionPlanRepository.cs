using LegacyRenewalApp.Models;

namespace LegacyRenewalApp.Repositories;

public interface ISubscriptionPlanRepository
{
    SubscriptionPlan GetByCode(string code);
}