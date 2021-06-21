using System.Threading.Tasks;
using CoreTests.Health.Mocks;
using FluentAssertions;
using NetFusion.Bootstrap.Health;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

namespace CoreTests.Health
{
    /// <summary>
    /// Unit tests asserting the correct reporting of the composite-application's health.
    /// </summary>
    public class HealthCheckTests
    {
        /// <summary>
        /// For the composite-application to have a Healthy status, all plugin modules must
        /// report the aspects they manage are healthy.
        /// </summary>
        [Fact]
        public Task CompositeAppHealthy_IfAllPluginModulesHealthy()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = fixture.Arrange.Container(c =>
                {
                    var corePlugin = new MockCorePlugin();
                    corePlugin.AddModule<HealthCheckModuleOne>();
                    corePlugin.AddModule<HealthCheckModuleTwo>();

                    c.RegisterPlugin<MockHostPlugin>();
                    c.RegisterPlugins(corePlugin);
                }).Act
                    .OnModule<HealthCheckModuleOne>(m => m.HealthCheckStatus = HealthCheckStatusType.Healthy)
                    .OnModule<HealthCheckModuleTwo>(m => m.HealthCheckStatus = HealthCheckStatusType.Healthy);

                await testResult.Assert.CompositeAppAsync(async a =>
                {
                    var healthCheck = await a.GetHealthCheckAsync();
                    healthCheck.OverallHealth.Should().Be(HealthCheckStatusType.Healthy);
                });
            });
        }

        /// <summary>
        /// If any plugin module reports a given aspect being Degraded, the overall status of the
        /// composite-application is degraded.
        /// </summary>
        [Fact]
        public Task CompositeAppDegraded_IfAnyPluginModuleDegraded()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = fixture.Arrange.Container(c =>
                    {
                        var corePlugin = new MockCorePlugin();
                        corePlugin.AddModule<HealthCheckModuleOne>();
                        corePlugin.AddModule<HealthCheckModuleTwo>();

                        c.RegisterPlugin<MockHostPlugin>();
                        c.RegisterPlugins(corePlugin);
                    }).Act
                    .OnModule<HealthCheckModuleOne>(m => m.HealthCheckStatus = HealthCheckStatusType.Healthy)
                    .OnModule<HealthCheckModuleTwo>(m => m.HealthCheckStatus = HealthCheckStatusType.Degraded);

                await testResult.Assert.CompositeAppAsync(async a =>
                {
                    var healthCheck = await a.GetHealthCheckAsync();
                    healthCheck.OverallHealth.Should().Be(HealthCheckStatusType.Degraded);
                });
            });
        }

        /// <summary>
        /// If any plugin module reports a given aspect being Unhealthy, the overall status of the
        /// composite-application is unhealthy.
        /// </summary>
        [Fact]
        public Task CompositeAppUnhealthy_IfAnyPluginModuleUnhealthy()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = fixture.Arrange.Container(c =>
                    {
                        var corePlugin = new MockCorePlugin();
                        corePlugin.AddModule<HealthCheckModuleOne>();
                        corePlugin.AddModule<HealthCheckModuleTwo>();

                        c.RegisterPlugin<MockHostPlugin>();
                        c.RegisterPlugins(corePlugin);
                    }).Act
                    .OnModule<HealthCheckModuleOne>(m => m.HealthCheckStatus = HealthCheckStatusType.Healthy)
                    .OnModule<HealthCheckModuleTwo>(m => m.HealthCheckStatus = HealthCheckStatusType.Unhealthy);

                await testResult.Assert.CompositeAppAsync(async a =>
                {
                    var healthCheck = await a.GetHealthCheckAsync();
                    healthCheck.OverallHealth.Should().Be(HealthCheckStatusType.Unhealthy);
                });
            });
        }

        /// <summary>
        /// If there are multiple non-healthy statuses reported by one or more modules, the overall
        /// status of the composite-application will be that of the more severe status.
        /// </summary>
        [Fact]
        public Task OverallAllCompositeAppHealth_IsMostSevereStatusType()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = fixture.Arrange.Container(c =>
                    {
                        var corePlugin = new MockCorePlugin();
                        corePlugin.AddModule<HealthCheckModuleOne>();
                        corePlugin.AddModule<HealthCheckModuleTwo>();

                        c.RegisterPlugin<MockHostPlugin>();
                        c.RegisterPlugins(corePlugin);
                    }).Act
                    .OnModule<HealthCheckModuleOne>(m => m.HealthCheckStatus = HealthCheckStatusType.Unhealthy)
                    .OnModule<HealthCheckModuleTwo>(m => m.HealthCheckStatus = HealthCheckStatusType.Degraded);

                await testResult.Assert.CompositeAppAsync(async a =>
                {
                    var healthCheck = await a.GetHealthCheckAsync();
                    healthCheck.OverallHealth.Should().Be(HealthCheckStatusType.Unhealthy);
                });
            });
        }
    }
}