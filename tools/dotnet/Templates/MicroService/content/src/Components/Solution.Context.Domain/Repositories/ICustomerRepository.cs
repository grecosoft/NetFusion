using System;
using System.Threading.Tasks;
using Solution.Context.Domain.Entities;

namespace Solution.Context.Domain.Repositories 
{
    public interface ICustomerRepository
    {
        Task<Customer> ReadAsync(Guid id);
        Task SaveAsync(Customer customer);
    }
}