using System.Threading.Tasks;
using Service.Domain.Entities;

namespace Service.Domain.Repositories
{
    public interface ICustomerRepository
    {
        Task AddCustomerAsync(Customer customer);
        Task<Customer> ReadCustomerAsync(string customerId);
        Task UpdateCustomerAsync(Customer customer);
    }
}