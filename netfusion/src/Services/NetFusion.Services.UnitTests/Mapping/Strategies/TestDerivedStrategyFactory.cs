using NetFusion.Services.Mapping;
using NetFusion.Services.UnitTests.Mapping.Entities;

namespace NetFusion.Services.UnitTests.Mapping.Strategies;

public class TestDerivedStrategyFactory : IMappingStrategyFactory
{
    public IEnumerable<IMappingStrategy> GetStrategies()
    {
        yield return DelegateMap.Map((Customer s) => new CustomerSummary
        {
            Description = $"{s.FirstName}-{s.LastName}",
            FirstName = s.FirstName,
            LastName = s.LastName
        });
                
        yield return DelegateMap.Map((Car s) =>
                
            new CarSummary {
                Description = $"{s.Make}-{s.Model}",
                Make = s.Make,
                Model = s.Model
            });
    }
}