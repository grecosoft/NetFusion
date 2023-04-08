using NetFusion.Services.Mapping;
using NetFusion.Services.UnitTests.Mapping.Entities;

namespace NetFusion.Services.UnitTests.Mapping.Strategies;

public class TestMappingStrategyFactory : IMappingStrategyFactory
{
    public IEnumerable<IMappingStrategy> GetStrategies()
    {
        yield return DelegateMap.Map((TestMapTypeOne s) => new TestMapTypeTwo
        {
            Sum = s.Values.Sum(),
            Min =  s.Values.Min(),
            Max = s.Values.Max()
        });
    }
}