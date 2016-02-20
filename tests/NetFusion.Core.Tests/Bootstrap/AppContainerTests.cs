using FluentAssertions;
using Xunit;

namespace NetFusion.Core.Tests.Bootstrap
{
    public class AppContainerTests
    {
        [Fact]
        public void CreatedAppContainerCanBeReferenced()
        {
            true.Should().BeTrue();
        }
    }
}
