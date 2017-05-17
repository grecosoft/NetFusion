using FluentAssertions;
using NetFusion.Common.Extensions.Collection;
using System.Collections.Generic;
using Xunit;

namespace CommonTests.Extensions
{
    public class DictionaryTests
    {
        [Fact (DisplayName = "MissingKeyNoDefault_ReturnsNull")]
        public void MissingKeyNoDefault_ReturnsNull()
        {
            var dic = new Dictionary<string, object> { { "a", new { } } };
            dic.GetOptionalValue("b").Should().BeNull();
        }

        [Fact(DisplayName = "NoKey_DefaultSpecified_ReturnsDefaultValue")]
        public void NoKey_DefaultSpecified_ReturnsDefaultValue()
        {
            var defaultValue = new { };
            var dic = new Dictionary<string, object> { { "a", new { } } };
            dic.GetOptionalValue("b", defaultValue).Should().BeSameAs(defaultValue);
        }

        [Fact(DisplayName = "KeyExists_ValueIsReturned")]
        public void KeyExists_ValueIsReturned()
        {
            var expectedVal = new { };
            var dic = new Dictionary<string, object> { { "a", expectedVal } };
            dic.GetOptionalValue("a").Should().BeSameAs(expectedVal);
        }
    }
}
