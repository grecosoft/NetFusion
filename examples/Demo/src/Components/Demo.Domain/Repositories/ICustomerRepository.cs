using System;
using System.Threading.Tasks;
using Demo.Domain.Entities;

namespace Demo.Domain.Repositories 
{
    public interface ICustomerRepository
    {
        Task<Customer> ReadAsync(Guid id);
        Task SaveAsync(Customer customer);
    }
}