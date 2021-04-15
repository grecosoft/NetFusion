using System;
using System.Collections;
using FluentAssertions;
using NetFusion.Common.Extensions.Collections;
using Xunit;

namespace CommonTests.Extensions.Collections
{
    public class SetTests
    {
        [Fact]
        public void GivenSingleValue_DetermineInSet()
        {
            const int value = 6;
            value.InSet(6, 8, 12).Should().BeTrue();
            value.InSet(6, 6, 12).Should().BeTrue();
            value.InSet(55, 99).Should().BeFalse();
        }

        [Fact]
        public void GivenValues_DetermineAnyInSet()
        {
            var values = new[] { 3, 10, 34, 77 };
            values.ContainsAny(10, 34).Should().BeTrue();
            values.ContainsAny(55).Should().BeFalse();
        }

        [Fact]
        public void GivenValues_AndComparer_DetermineAnyInSet()
        {
            var values = new[] {"Value1", "VALUE2", "value3"};
            values.ContainsAny(StringComparer.OrdinalIgnoreCase, "value2").Should().BeTrue();
        }
    }
}
