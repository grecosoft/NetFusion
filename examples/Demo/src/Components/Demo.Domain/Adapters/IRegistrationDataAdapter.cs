using System.Threading.Tasks;

namespace Demo.Domain.Adapters
{
    public interface IRegistrationDataAdapter
    {
        Task<AutoInfo[]> GetValidModelsAsync(int forYear);
    }
}
