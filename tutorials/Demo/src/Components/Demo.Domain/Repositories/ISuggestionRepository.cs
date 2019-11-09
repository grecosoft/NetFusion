using System.Threading.Tasks;
using Demo.Domain.Entities;

namespace Demo.Domain.Repositories
{
    public interface ISuggestionRepository
    {
        Task<AutoSuggestion[]> LookupSuggestedAutos(string state, int age);
    }
}