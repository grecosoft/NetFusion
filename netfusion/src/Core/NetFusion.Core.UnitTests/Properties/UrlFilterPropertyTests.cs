using System.Net;
using NetFusion.Common.Base.Properties;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Core.TestFixtures.Plugins;

namespace NetFusion.Core.UnitTests.Properties;

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