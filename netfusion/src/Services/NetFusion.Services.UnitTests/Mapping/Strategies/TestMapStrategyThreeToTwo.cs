using NetFusion.Services.Mapping;
using NetFusion.Services.UnitTests.Mapping.Entities;

namespace NetFusion.Services.UnitTests.Mapping.Strategies;

public class TestMapStrategyThreeToTwo : MappingStrategy<TestMapTypeThree, TestMapTypeTwo>
{
    protected override TestMapTypeTwo SourceToTarget(TestMapTypeThree source)
    {
        var validValues = source.Values.Where(v => v <= source.MaxAllowedValue).ToArray();
            
        return new TestMapTypeTwo
        {
            Min = validValues.Min(),
            Max = validValues.Max(),
            Sum = validValues.Sum()
        };
    }
}