namespace Service.Domain.Repositories
{
    using System.Threading.Tasks;
    using Service.Domain.Entities;

    public interface ICustomerRepository
    {
        Task AddCustomerAsync(Customer customer);
        Task<Customer> ReadCustomerAsync(string customerId);
        Task UpdateCustomerAsync(Customer customer);
    }
}