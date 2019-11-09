using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.Domain.Entities;
using Demo.Domain.Repositories;

namespace Demo.Infra.Repositories
{
    public class SuggestionRepository : ISuggestionRepository
    {
        private static readonly List<AutoSuggestion> Data = new List<AutoSuggestion>();

        static SuggestionRepository()
        {
            Data.Add(new AutoSuggestion("Honda", "Accord", 2010, "Silver"));
        }

        public Task<AutoSuggestion[]> LookupSuggestedAutos(string state, int age)
        {
            return Task.FromResult(Data.ToArray());
        }
    }
}