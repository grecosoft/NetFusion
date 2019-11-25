using System.Threading.Tasks;

namespace Demo.Domain.Repositories
{
    public interface IStateInfoRepository
    {
        Task<string> Add(StateInfo stateInfo);
        Task<StateInfo> Read(string state);
    }
}
