using System.Threading.Tasks;

namespace Demo.Infra
{
    public interface ISalesDataAdapter
    {
        Task<AutoSalesInfo[]> GetInventory(string make, int year);
    }
}