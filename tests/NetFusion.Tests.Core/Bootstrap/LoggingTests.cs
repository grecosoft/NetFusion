using FluentAssertions;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Bootstrap.Testing;
using NetFusion.Tests.Core.Bootstrap.Mocks;
using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace NetFusion.Tests.Core.Bootstrap
{
    /// <summary>
    /// Unit tests for container logging configuration and for testing 
    /// the handling of runtime exceptions.
    /// </summary>
    public class LoggingUnitTests
    {
        /// <summary>
        /// The host application doesn't need to configure a logger.
        /// If no logger is configured, all containers exceptions area raised
        /// and a null-logger implementation is used.
        /// </summary>
        [Fact]
        public void IfLogConfigNotSpecified_NullLoggerUsed()
        {
            ContainerSetup
                .Arrange((HostTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>();
                })
                .Act(c => { c.Build(); })
                .Assert((AppContainer c) =>
                {
                    c.Logger.Should().NotBeNull();
                    c.Logger.Should().BeOfType<NullLogger>();
                });
        }

        /// <summary>
        /// Null reference can't be set for the logger.
        /// </summary>
        [Fact]
        public void IfLoggerNotSpecified_ExceptionRaised()
        {
            ContainerSetup
                .Arrange((HostTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>();
                })
                .Act(c =>
                {
                    c.WithConfig<LoggerConfig>(lc => { lc.SetLogger(null); });
                    c.Build();
                })
                .Assert((c, e) =>
                {
                    e.Should().NotBeNull();
                    e.Should().BeOfType<ArgumentNullException>();
                });
        }

        /// <summary>
        /// The application host can specify a custom logger during
        /// application container initialization.
        /// </summary>
        [Fact]
        public void IfLogConfigSpecified_InstanceUsed()
        {
            var logger = new MockLogger();

            ContainerSetup
                .Arrange((HostTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>();
                })
                .Act(c => 
                {
                    c.WithConfig<LoggerConfig>(lc => lc.SetLogger(logger));
                    c.Build();
                })
                .Assert((AppContainer c) =>
                {
                    c.Logger.Should().NotBeNull();
                    c.Logger.Should().BeSameAs(logger);
                });
        }

        /// <summary>
        /// After the container is built, a detailed log of the container's
        /// composition accessed.  The log is a nested dictionary and
        /// anonymous types that can be easily serialized to JSON.  
        /// </summary>
        [Fact]
        public void AppContainerLogIsCreated()
        {
            ContainerSetup
                .Arrange((HostTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>();
                })
                .Act(c => c.Build())
                .Assert((AppContainer c) =>
                {
                    c.Log.Should().NotBeNull();
                    c.Log.ContainsKey("Searched-Plugin-Assemblies").Should().BeTrue();
                    c.Log.ContainsKey("Application-Host").Should().BeTrue();
                    c.Log.ContainsKey("Component-Plugins").Should().BeTrue();
                    c.Log.ContainsKey("Core-Plugins").Should().BeTrue();
                });
        }

        /// <summary>
        /// During the bootstrap process, each plug-in module can add additional
        /// plug-in specific logs to the container log.
        /// </summary>
        [Fact]
        public void EachModuleCanAddToAppContainerLog()
        {
            ContainerSetup
                .Arrange((HostTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockLoggingModule>();
                })
                .Act(c => c.Build())
                .Assert((AppContainer c) =>
                {
                    c.Log.Should().NotBeNull();
                    var appHostLog = c.Log["Application-Host"];

                    var moduleLogs = (appHostLog as IDictionary<string, object>)["Plugin-Modules"];
                    moduleLogs.Should().NotBeNull();

                    dynamic moduleLog = (moduleLogs as IDictionary)["NetFusion.Tests.Core.Bootstrap.Mocks.MockLoggingModule"];

                    ((object)moduleLog).Should().NotBeNull();

                    IDictionary values = moduleLog.Log;
                    values.Contains("Log-Msg").Should().BeTrue();
                });
        }
    }
}
