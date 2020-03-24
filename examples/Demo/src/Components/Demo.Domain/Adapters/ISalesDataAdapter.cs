using System.Threading.Tasks;

namespace Demo.Domain.Adapters
{
    public interface ISalesDataAdapter
    {
        Task<AutoSalesInfo[]> GetInventory(string make, int year);
    }
}
