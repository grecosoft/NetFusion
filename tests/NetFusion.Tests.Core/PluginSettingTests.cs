using Autofac;
using Autofac.Core;
using FluentAssertions;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Testing;
using NetFusion.Core.Tests.Settings.Mocks;
using NetFusion.Settings;
using NetFusion.Settings.Configs;
using NetFusion.Tests.Core.Bootstrap;
using System;
using Xunit;

namespace NetFusion.Tests.Core.Settings
{
    /// <summary>
    /// Unit tests for the settings core plug-in used to load application settings
    /// as they are dependency injected into consuming components.
    /// </summary>
    public class PluginSettingTests
    {

        /// <summary>
        /// If a NetFusionConfig is not specified in code and a configuration is 
        /// specified within the application's configuration file, it will be used.
        /// </summary>
        [Fact]
        public void HostAppConfigUsedIfSpecifiedAndNotManuallySet()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.SetupHostApp();
                    config.SetupSettingsPlugin();
                })
                .Act(c =>
                {
                    c.WithConfig<NetFusionConfigSection>(cfg =>
                    {
                        cfg.HostConfig = new HostConfigElement { Environment = EnvironmentTypes.Development };
                    });
                    c.Build();
                })
                .AssertConfig((NetFusionConfig cfg) =>
                {
                    cfg.Environment.Should().Be(EnvironmentTypes.Development);
                });
        }

        /// <summary>
        /// The host application can only specify open generic setting initializers
        /// when configuring the application container.
        /// </summary>
        [Fact]
        public void OnlyOpenGenericInitializersCanBeConfigured()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.SetupHostApp();
                    config.SetupSettingsPlugin();
                })
                .Act(c =>
                {

                    c.WithConfig<NetFusionConfig>(cfg => cfg.AddSettingsInitializer(typeof(MockSettingInitOne)));
                    c.Build();
                })
                .Assert((c, e) =>
                {
                    e.Should().BeOfType<ArgumentException>()
                        .Subject.Message.Should().Contain("open");
                });
        }

        /// <summary>
        /// A given setting class cannot have more than one specific initializer
        /// specified within an application centric plug-in.
        /// </summary>
        [Fact]
        public void CannotHaveDuplicateSettingSpecificInitializers()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.SetupHostApp();
                    config.SetupSettingsPlugin();

                    // Add two different setting initializer classes that are
                    // for initializing the same setting type.
                    config.AddPlugin<MockAppComponentPlugin>()
                        .AddPluginType<MockSettings>()
                        .AddPluginType<MockSettingInitOne>()
                        .AddPluginType<MockSettingInitTwo>();
                })
                .Act(c =>
                {
                    c.Build();
                })
                .Assert((c, e) =>
                {
                    e.Should().BeOfType<ContainerException>()
                        .Subject.Message.Should().Contain("duplicate setting initializers");
                });
        }

        /// <summary>
        /// A specific settings initializer will be used to load a setting's values if present.
        /// </summary>
        [Fact]
        public void SettingSpecificInitializerUsedIfPresent()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.SetupHostApp();
                    config.SetupSettingsPlugin();

                    config.AddPlugin<MockAppComponentPlugin>()
                        .AddPluginType<MockSettings>()
                        .AddPluginType<MockSettingInitOne>()
                        .AddPluginType(typeof(MockGenericIntOne<>));
                })
                .Act(c =>
                {
                    c.WithConfig<NetFusionConfig>(ac => ac.AddSettingsInitializer(typeof(MockGenericIntOne<>)));
                    c.Build();
                    c.Start();
                })
                .Assert((AppContainer c) =>
                {
                    var settings = c.Services.Resolve<MockSettings>();
                    settings.SettingValue.Should().Be(nameof(MockSettingInitOne));
                });
        }

        /// <summary>
        /// If there are no setting specific initializers, then the registered
        /// generic setting initializers will be processed.
        /// </summary>
        [Fact]
        public void HostRegistedGenericInitialzerUsedIfSpecificNotFound()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.SetupHostApp();
                    config.SetupSettingsPlugin();

                    config.AddPlugin<MockAppComponentPlugin>()
                        .AddPluginType<MockSettings>()
                        .AddPluginType(typeof(MockGenericIntOne<>));
                })
                .Act(c =>
                {
                    c.WithConfig<NetFusionConfig>(ac => ac.AddSettingsInitializer(typeof(MockGenericIntOne<>)));
                    c.Build();
                    c.Start();
                })
                .Assert((AppContainer c) =>
                {
                    var settings = c.Services.Resolve<MockSettings>();
                    settings.Should().NotBeNull();
                    settings.SettingValue.Should().Be(typeof(MockGenericIntOne<>).Name);
                });
        }
        
        /// <summary>
        /// An application setting can specify that an initializer is not required.
        /// By default, an initializer is required.
        /// </summary>
        [Fact]
        public void PluginSettingInitializerCanBeOptional()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.SetupHostApp();
                    config.SetupSettingsPlugin();

                    config.AddPlugin<MockAppComponentPlugin>()
                        .AddPluginType<MockOptionalSettings>();
                })
                .Act(c =>
                {
                    c.Build();
                })
                .Assert((AppContainer c) =>
                {
                    var settings = c.Services.Resolve<MockOptionalSettings>();
                    settings.Should().NotBeNull();
                    settings.DefaultValue1.Should().Be("Value1");
                });
        }

        /// <summary>
        /// By default an exception will be thrown if a setting initializer can't be found
        /// for a application setting.
        /// </summary>
        [Fact]
        public void PluginSettingCanBeSpecfiedToRequireInitializer()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.SetupHostApp();
                    config.SetupSettingsPlugin();

                    config.AddPlugin<MockAppComponentPlugin>()
                        .AddPluginType<MockSettings>();
                })
                .Act(c =>
                {
                    c.Build();
                    var settings = c.Services.Resolve<MockSettings>();
                })
                .Assert((AppContainer c, Exception e) =>
                {
                    e.Should().BeOfType<DependencyResolutionException>();
                });
        }
    }
}

