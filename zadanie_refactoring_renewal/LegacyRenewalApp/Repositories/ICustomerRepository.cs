using LegacyRenewalApp.Models;

namespace LegacyRenewalApp.Repositories;

public interface ICustomerRepository
{
    Customer GetById(int customerId);
}