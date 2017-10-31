using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using NetFusion.Bootstrap.Configuration;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace CoreTests.Settings.Mocks
{
    public class SettingsConfigTests
    {
        /// <summary>
        /// The application container can also be used by an executable other than a web host.  
        /// Therefore, the configuration checks both the NETFUSION_ENVIRONMENT and ASPNETCORE_ENVIRONMENT
        /// variables to determine the current environment of the executing application.  Also, if specified
        /// for both variables, the NETFUSTION_ENVIROMENT variable is used.
        /// </summary>
        [Fact]
        public void AspMvc_And_Netfusion_CheckedForEnvironment()
        {
            Environment.SetEnvironmentVariable("NETFUSION_ENVIRONMENT", null);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "AspConfigEnv");
            EnvironmentConfig.EnvironmentName.Should().Be("AspConfigEnv");
            Environment.SetEnvironmentVariable("NETFUSION_ENVIRONMENT", "NetFusionConfigEnv");
            EnvironmentConfig.EnvironmentName.Should().Be("NetFusionConfigEnv");
        }

        /// <summary>
        /// When retrieving a configuration settings, the default configuration is to
        /// resolve the value by looking in order the following files:
        /// appSettings.MachineName.json
        /// appSettings.Environment.json
        /// appSettings.Json
        /// </summary>
        [Fact]
        public void ConfigFilesRegistered_CorrectOrder()
        {
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
        [Fact]
        public void MsConfigExtenions_SetToUse_CurrentExecutionDirectory()
        {
            string expectedDirectoryName = Directory.GetCurrentDirectory();

            ConfigurationBuilder configBuilder = new ConfigurationBuilder();
            configBuilder.AddDefaultAppSettings();

            configBuilder.Properties.Keys.Should().Contain("FileProvider");
            var fileProvider = configBuilder.Properties["FileProvider"].Should().BeOfType<PhysicalFileProvider>().Subject;
            fileProvider.Root.Should().Equals(expectedDirectoryName);       
        }
    }
}
