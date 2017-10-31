using CoreTests.Bootstrap.Mocks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NetFusion.Bootstrap.Configuration;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using System;
using System.Linq;
using Xunit;

namespace CoreTests.Bootstrap
{
    /// <summary>
    /// The host application can register container configurations during the 
    /// bootstrap process.  All container configurations belong to a specific
    /// plug-in.  When the application is composed, all provided configurations
    /// are associated with the plug-in defining them.  The configurations can
    /// be referenced within the plug-in modules.
    /// </summary>
    public class ConfigurationTests
    {
        /// <summary>
        /// Host application can provide an instance of a configuration object.
        /// </summary>
        [Fact(DisplayName = nameof(SpecifiedPluginConfiguration_AssociatedWithPlugin))]
        public void SpecifiedPluginConfiguration_AssociatedWithPlugin()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockPluginConfig>();
                })
                .Test(c =>
                {
                    c.WithConfig(new MockPluginConfig());
                    c.Build();
                },
                (CompositeApplication ca) =>
                {
                    ca.AppHostPlugin.PluginConfigs.Should().HaveCount(1);
                    ca.AppHostPlugin.PluginConfigs.First().Should().BeOfType<MockPluginConfig>();
                });
        }

        /// <summary>
        /// The host application can also provide a configuration by using factory method.   
        /// </summary>
        [Fact(DisplayName = nameof(PluginConfiguration_CanBeInitialized))]
        public void PluginConfiguration_CanBeInitialized()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockPluginConfig>();
                })
                .Test(c =>
                {
                    c.WithConfig<MockPluginConfig>((confg) => confg.ConfigValue = "TEST_VALUE");
                    c.Build();
                },
                (CompositeApplication ca) =>
                {
                    ca.AppHostPlugin.PluginConfigs.Should().HaveCount(1);

                    var config = ca.AppHostPlugin.PluginConfigs.First();

                    config.Should().BeOfType<MockPluginConfig>();
                    (config as MockPluginConfig).ConfigValue.Should().Be("TEST_VALUE");
                });
        }

        [Fact(DisplayName = nameof(AfterContainerBuild_ConfigurationCannotBeAdded))]
        public void AfterContainerBuild_ConfigurationCannotBeAdded()
        {
            ContainerSetup
               .Arrange((TestTypeResolver config) =>
               {
                   var appHostPlugin = new MockAppHostPlugin { };
                   config.AddPlugin(appHostPlugin);
               })
               .Test(c =>
               {
                   c.Build();
                   c.WithConfig<MockPluginConfig>();
               },
               (c, e) =>
               {
                   e.Should().NotBeNull();
                   e.Should().BeOfType<ContainerException>();
               });
        }

        [Fact(DisplayName = nameof(ApplicationVariable_CanBeRead_ToDetermineEnvironment))]
        public void ApplicationVariable_CanBeRead_ToDetermineEnvironment()
        {
            Environment.SetEnvironmentVariable("NETFUSION_ENVIRONMENT", null);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentNames.Production);
            EnvironmentConfig.EnvironmentName.Should().Be(EnvironmentNames.Production);          

            Environment.SetEnvironmentVariable("NETFUSION_ENVIRONMENT", EnvironmentNames.Staging);
            EnvironmentConfig.EnvironmentName.Should().Be(EnvironmentNames.Staging);
        }

        [Fact(DisplayName = nameof(EvironmentVariableProvider_AddedIfSecified))]
        public void EvironmentVariableProvider_AddedIfSecified()
        {
            var expectedValue = Guid.NewGuid().ToString();

            Environment.SetEnvironmentVariable("TEST_ENV_VAR", expectedValue);

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddEnvironmentVariables();
            configBuilder.AddDefaultAppSettings();

            var config = configBuilder.Build();
            config["TEST_ENV_VAR"].Should().NotBeNull();
        }

        [Fact(DisplayName = nameof(DefaultConfigurationProviders_AddedInCorrectOrder))]
        public void DefaultConfigurationProviders_AddedInCorrectOrder()
        {

        }
    }
}
