using RefArch.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RefArch.Domain.Samples.MongoDb
{
    public interface IExampleRepository
    {
        Task AddCustomerAsync(Customer custoer);
        Task<List<Customer>> ListCustomersAsync();
    }
}
