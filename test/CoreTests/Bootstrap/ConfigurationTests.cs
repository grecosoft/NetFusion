using CoreTests.Bootstrap.Mocks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using NetFusion.Bootstrap.Configuration;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace CoreTests.Bootstrap
{
    /// <summary>
    /// The host application can register container configurations during the bootstrap process.  
    /// All container configurations belong to a specific plug-in.  When the application is composed, 
    /// all provided configurations are associated with the plug-in defining them.  The configurations 
    /// can be referenced within the plug-in modules.
    /// </summary>
    public class ConfigurationTests
    {
        /// <summary>
        /// Host application can provide an instance of a configuration object.  When the container 
        /// is bootstrapped, each specified configuration is associated with the plug-in instance 
        /// where defined.  This is only of concern to a plug-in developer needs access to one of
        /// its defined configurations.
        /// </summary>
        [Fact(DisplayName = "Specified Configuration associated with Plug-in")]
        public void SpecifiedConfiguration_AssociatedWithPlugin()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange.Resolver(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockPluginConfig>();
                })
                .Act.OnContainer(c =>
                {
                    c.WithConfig(new MockPluginConfig());
                    c.Build();
                })
                .Assert
                    .CompositeApp(ca =>
                    {
                        ca.AppHostPlugin.PluginConfigs.Should().HaveCount(1);
                        ca.AppHostPlugin.PluginConfigs.First().Should().BeOfType<MockPluginConfig>();
                    })
                    .Plugin<MockAppHostPlugin>(p => {
                        p.PluginConfigs.Should().HaveCount(1);
                        p.PluginConfigs.First().Should().BeOfType<MockPluginConfig>();
                    });
            });              
        }

        /// <summary>
        /// When developing a plug-in that has associated configurations, they are most often
        /// accessed from within one or more modules.
        /// </summary>
        [Fact(DisplayName = "Plug-in Developer can access Configuration from Module")]
        public void PluginDeveloper_CanAccess_ConfigurationFromModule()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange.Resolver(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockPluginModule>()
                        .AddPluginType<MockPluginConfig>();
                })
                .Act.OnContainer(c =>
                {
                    c.WithConfig(new MockPluginConfig());
                    c.Build();
                })
                .Assert.PluginModule<MockPluginModule>(m => {
                        m.Context.Plugin.GetConfig<MockPluginConfig>().Should().NotBeNull();
                });
            });
        }

        /// <summary>
        /// The host application can also provide a configuration by using factory method.   
        /// </summary>
        [Fact(DisplayName = "Plug-in Configuration can be initialized using Factory")]
        public void PluginConfiguration_CanBeInitialized_UsingFactory()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange.Resolver(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockPluginConfig>();
                })
                .Act.OnContainer(c =>
                {
                    c.WithConfig<MockPluginConfig>((confg) => confg.ConfigValue = "TEST_VALUE");
                    c.Build();
                })
                .Assert.CompositeApp(ca => {
                    ca.AppHostPlugin.PluginConfigs.Should().HaveCount(1);

                    var config = ca.AppHostPlugin.PluginConfigs.First();

                    config.Should().BeOfType<MockPluginConfig>();
                    (config as MockPluginConfig).ConfigValue.Should().Be("TEST_VALUE");
                });               
            });               
        }
        
        [Fact(DisplayName = "After Container built Configuration cannot be Added")]
        public void AfterContainerBuilt_ConfigurationCannotBeAdded()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange.Resolver(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockPluginConfig>();
                })
                .Act.OnContainer(c =>
                {
                    c.Build();
                    c.WithConfig<MockPluginConfig>();
                })
                .Assert.Exception<ContainerException>(e => {
                    e.Message.Contains("Container has already been built.");
                });
            });
        }

        /// <summary>
        /// The application container can also be used by an executable other than a web host.  
        /// Therefore, the configuration checks both the NETFUSION_ENVIRONMENT and ASPNETCORE_ENVIRONMENT
        /// variables to determine the current environment of the executing application.  Also, if specified
        /// for both variables, the NETFUSTION_ENVIROMENT variable is used.
        /// </summary>
        [Fact(DisplayName = "Environment Variable can be read to determine Execution Context")]
        public void EnvironmentVariable_CanBeRead_ToDetermineExecutionContext()
        {
            // Clear current environment variables:
            Environment.SetEnvironmentVariable("NETFUSION_ENVIRONMENT", null);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "AspConfigEnv");
            EnvironmentConfig.EnvironmentName.Should().Be("AspConfigEnv");

            Environment.SetEnvironmentVariable("NETFUSION_ENVIRONMENT", "NetFusionConfigEnv");
            EnvironmentConfig.EnvironmentName.Should().Be("NetFusionConfigEnv");
        }

        /// <summary>
        /// When retrieving a configuration settings, the default configuration is to resolve the 
        /// value by looking in order the following files:
        /// appSettings.MachineName.json
        /// appSettings.Environment.json
        /// appSettings.Json
        /// </summary>
        [Fact(DisplayName = "Configuration files Registered in correct Order")]
        public void ConfigFilesRegistered_InCorrectOrder()
        {
            // Clear current environment variables:
            Environment.SetEnvironmentVariable("NETFUSION_ENVIRONMENT", null);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);

            Environment.SetEnvironmentVariable("NETFUSION_ENVIRONMENT", "UnitTest");
            ConfigurationBuilder configBuilder = new ConfigurationBuilder();

            configBuilder.AddDefaultAppSettings();

            configBuilder.Sources.Should().HaveCount(3);
            GetFileAtPosition(configBuilder, 0).Should().Be("appsettings.json");
            GetFileAtPosition(configBuilder, 1).Should().Be("appsettings.UnitTest.json");
            GetFileAtPosition(configBuilder, 2).Should().Be($"appsettings.{Environment.MachineName}.json");


            string GetFileAtPosition(ConfigurationBuilder builder, int position)
                => builder.Sources.OfType<JsonConfigurationSource>().ElementAt(position)?.Path ?? "";

        }

        /// <summary>
        /// The default MS Configuration Extensions set by NetFusion if used specifies the application's
        /// current directory as the root location of the configuration files.  This allows the same
        /// configuration to be used between ASP.NET applications and Console executables.
        /// </summary>
        [Fact(DisplayName = "MS Configuration Extensions file-provider Location set.")]
        public void MsConfigExtenions_FileProvider_LocationSet()
        {
            string expectedDirectoryName = Directory.GetCurrentDirectory();

            ConfigurationBuilder configBuilder = new ConfigurationBuilder();
            configBuilder.AddDefaultAppSettings();

            configBuilder.Properties.Keys.Should().Contain("FileProvider");
            var fileProvider = configBuilder.Properties["FileProvider"].Should().BeOfType<PhysicalFileProvider>().Subject;
            fileProvider.Root.Should().Equals(expectedDirectoryName);
        }

        /// <summary>
        /// Not part of NetFusion but environment variables can be read as configuration settings
        /// if the AddEnvironmentVariables method is called on the configuration builder.
        /// </summary>
        [Fact(DisplayName = "Configuration Values can read from Environment Variables")]
        public void ConfigurationValues_CanReadFrom_EnvironmentVariables()
        {
            var expectedValue = Guid.NewGuid().ToString();

            Environment.SetEnvironmentVariable("TEST_ENV_VAR", expectedValue);

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddEnvironmentVariables();
            configBuilder.AddDefaultAppSettings();

            var config = configBuilder.Build();
            config["TEST_ENV_VAR"].Should().NotBeNull();
        }
    }
}
