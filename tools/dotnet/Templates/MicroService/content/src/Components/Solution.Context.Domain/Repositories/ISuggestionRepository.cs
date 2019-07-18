using System.Threading.Tasks;
using Solution.Context.Domain.Entities;

namespace Solution.Context.Domain.Repositories
{
    public interface ISuggestionRepository
    {
        Task<AutoSuggestion[]> LookupSuggestedAutos(string state, int age);
    }
}