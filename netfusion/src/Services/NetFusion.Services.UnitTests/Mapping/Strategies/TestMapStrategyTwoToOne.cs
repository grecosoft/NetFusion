using NetFusion.Services.Mapping;
using NetFusion.Services.UnitTests.Mapping.Entities;

namespace NetFusion.Services.UnitTests.Mapping.Strategies;

public class TestMapStrategyTwoToOne : MappingStrategy<TestMapTypeTwo, TestMapTypeOne>
{
    protected override TestMapTypeOne SourceToTarget(TestMapTypeTwo target) =>
        new()
        {
            Values = [target.Min, target.Max, target.Sum]
        };
}