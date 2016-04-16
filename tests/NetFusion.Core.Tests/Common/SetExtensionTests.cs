using FluentAssertions;
using NetFusion.Common.Extensions;
using Xunit;

namespace NetFusion.Core.Tests.Common
{
    public class SetExtensionTests
    {
        [Fact]
        public void InSet()
        {
            var value = 6;
            value.InSet(6, 8, 12).Should().BeTrue();
            value.InSet(55, 99).Should().BeFalse();
        }

        [Fact]
        public void ContainsAny()
        {
            var values = new[] { 3, 10, 34, 77 };
            values.ContainsAny(10, 34).Should().BeTrue();
            values.ContainsAny(55).Should().BeFalse();
        }
    }
}
