using System.Threading.Tasks;
using WebApiHost.MongoDB.Models;

namespace WebApiHost.MongoDB
{
    public interface ICustomerRepository
    {
        Task AddCustomerAsync(CustomerModel custoer);
        Task<CustomerModel[]> ListCustomersAsync();
    }
}
