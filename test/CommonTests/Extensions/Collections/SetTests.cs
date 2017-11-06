using FluentAssertions;
using NetFusion.Common.Extensions.Collections;
using Xunit;

namespace CommonTests.Extensions.Collections
{
    public class SetTests
    {
        [Fact(DisplayName = nameof(GivenSingleValue_DetermineInSet))]
        public void GivenSingleValue_DetermineInSet()
        {
            var value = 6;
            value.InSet(6, 8, 12).Should().BeTrue();
            value.InSet(6, 6, 12).Should().BeTrue();
            value.InSet(55, 99).Should().BeFalse();
        }

        [Fact (DisplayName = nameof(GivenValues_DetermineAnyInSet))]
        public void GivenValues_DetermineAnyInSet()
        {
            var values = new[] { 3, 10, 34, 77 };
            values.ContainsAny(10, 34).Should().BeTrue();
            values.ContainsAny(55).Should().BeFalse();
        }
    }
}
