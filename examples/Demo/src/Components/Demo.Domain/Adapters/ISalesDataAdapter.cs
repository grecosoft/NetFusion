using System.Threading.Tasks;
using Demo.Domain.Entities;

namespace Demo.Domain.Adapters
{
    public interface ISalesDataAdapter
    {
        Task<AutoSalesInfo[]> GetInventory(string make, int year);
    }
}
