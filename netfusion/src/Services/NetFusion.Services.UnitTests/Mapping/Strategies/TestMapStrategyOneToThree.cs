using NetFusion.Services.Mapping;
using NetFusion.Services.UnitTests.Mapping.Entities;

namespace NetFusion.Services.UnitTests.Mapping.Strategies;

public class TestMapStrategyOneToThree : MappingStrategy<TestMapTypeOne, TestMapTypeThree>
{
    protected override TestMapTypeThree SourceToTarget(TestMapTypeOne source)
    {
        return new TestMapTypeThree
        {
            MaxAllowedValue = source.Values.Max(),
            Values = source.Values
        };
    }
}