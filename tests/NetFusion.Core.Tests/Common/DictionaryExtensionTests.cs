using FluentAssertions;
using NetFusion.Common.Extensions;
using System.Collections.Generic;
using Xunit;

namespace NetFusion.Core.Tests.Common
{
    public class DictionaryExtensionTests
    {
        [Fact]
        public void MissingKeyNoDefault_ReturnsNull()
        {
            var dic = new Dictionary<string, object> { { "a", new { } } };
            dic.GetOptionalValue("b").Should().BeNull();
        }

        [Fact]
        public void MissingKeyDefaultSpecified_ReturnsDefault()
        {
            var expectedVal = new { };
            var dic = new Dictionary<string, object> { { "a", new { } } };
            dic.GetOptionalValue("b", expectedVal).Should().BeSameAs(expectedVal);
        }

        [Fact]
        public void KeyExists_ValueIsReturned()
        {
            var expectedVal = new { };
            var dic = new Dictionary<string, object> { { "a", expectedVal } };
            dic.GetOptionalValue("a").Should().BeSameAs(expectedVal);
        }
    }
}
