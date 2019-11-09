using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Demo.Domain.Entities;
using Demo.Domain.Repositories;

namespace Demo.Infra.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private static readonly List<Customer> Data = new List<Customer>();

        public Task<Customer> ReadAsync(Guid id)
        {
            var customer = Data.FirstOrDefault(c => c.Id == id);
            
            return Task.FromResult(customer);
        }

        public Task SaveAsync(Customer customer)
        {
            Data.RemoveAll(c => c.Id == customer.Id);
            Data.Add(customer);

            return Task.CompletedTask;
        }
    }
}