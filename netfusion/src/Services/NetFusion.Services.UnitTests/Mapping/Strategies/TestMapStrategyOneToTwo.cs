using NetFusion.Services.Mapping;
using NetFusion.Services.UnitTests.Mapping.Entities;

namespace NetFusion.Services.UnitTests.Mapping.Strategies;

public class TestMapStrategyOneToTwo : MappingStrategy<TestMapTypeOne, TestMapTypeTwo>
{
    protected override TestMapTypeTwo SourceToTarget(TestMapTypeOne source)
    {
        return new TestMapTypeTwo
        {
            Sum = source.Values.Sum(),
            Min = source.Values.Min(),
            Max = source.Values.Max()
        };
    }
}