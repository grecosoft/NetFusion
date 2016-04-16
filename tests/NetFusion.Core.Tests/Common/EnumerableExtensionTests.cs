using FluentAssertions;
using NetFusion.Common.Extensions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NetFusion.Core.Tests.Common
{
    public class EnumerableExtensionTests
    {
        [Fact]
        public void If_Null_Enumerable_Return_EmptySet()
        {
            const IEnumerable<int> nullEnumerable = null;
            var value = nullEnumerable.EmptyIfNull().ToList();
            value.Should().NotBeNull();
            value.Should().BeEmpty();
        }

        [Fact]
        public void Can_Create_Dictionary_With_Duplicate_Keys()
        {

        }
    }
}
