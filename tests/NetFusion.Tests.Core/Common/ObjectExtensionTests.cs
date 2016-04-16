using FluentAssertions;
using NetFusion.Common.Extensions;
using Xunit;

namespace NetFusion.Core.Tests.Common
{
    public class ObjectExtensionTests
    {
        [Fact]
        public void ToDictionary()
        {
            var obj = new
            {
                FirstValue = 10,
                SecondValue = "TestValue",
            };

            var objDictionary = obj.ToDictionary();
            objDictionary.Should().HaveCount(2);
            objDictionary.Keys.Should().Contain("FirstValue", "SecondValue");
            objDictionary["FirstValue"].Should().Be(10);
            objDictionary["SecondValue"].Should().Be("TestValue");
        }
    }
}
