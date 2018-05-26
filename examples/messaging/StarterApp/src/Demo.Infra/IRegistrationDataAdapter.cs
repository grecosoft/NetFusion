using System.Threading.Tasks;

namespace Demo.Infra
{
    public interface IRegistrationDataAdapter
    {
        Task<AutoInfo[]> GetValidModelsAsync(int forYear);
    }
}