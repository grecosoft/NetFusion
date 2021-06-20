using System.Net;
using FluentAssertions;
using NetFusion.Base.Properties;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

namespace CommonTests.Base.Properties
{
    public class UrlFilterPropertyTests
    {
        [Fact]
        public void UrlFilters_CanBeRegistered()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                    }).Act.OnCompositeApp(ca =>
                    {
                        ca.Properties.AddLogUrlFilter("/mgt/health-check", HttpStatusCode.OK);
                    })
                    .Assert.CompositeApp(ca =>
                    {
                        ca.Properties.IsLogUrlFilter("/mgt/health-check", HttpStatusCode.OK)
                            .Should().BeTrue();
                    });
            });
        }
    }
}