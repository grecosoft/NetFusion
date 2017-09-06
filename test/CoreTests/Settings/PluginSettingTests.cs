using Autofac;
using Autofac.Core;
using BootstrapTests.Settings.Mocks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NetFusion.Bootstrap.Configuration;
using NetFusion.Bootstrap.Container;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using NetFusion.Utilities.Validation;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CoreTests.Settings
{
    /// <summary>
    /// Unit tests for testing the injecting of configuration setting classes
    /// that are loaded from the providers specified on the ConfigurationBuilder.
    /// </summary>
    public class PluginSettingTests
    {
        /// <summary>
        /// A configuration setting class deriving from IAppSetting and marked with the
        /// ConfigurationSection attribute will be loaded from the associated section.
        /// </summary>
        [Fact(DisplayName = nameof(CanInjectSettings_DirectlyIntoConsumer))]
        public void CanInjectSettings_DirectlyIntoConsumer()
        {
            ContainerSetup
                .Arrange((TestTypeResolver resolver) =>
                {
                    resolver.DefaultSettingsConfig();
                    resolver.AddPlugin<MockCorePlugin>().AddPluginType<MockSetttings>();
                })
                
                .Test(c =>
                {
                    c.WithConfig<EnvironmentConfig>(settings => {

                        AddInMemorySettings(settings);
                    });

                    c.Build();
                },
                (IAppContainer c) =>
                {
                    var settings = c.Services.Resolve<MockSetttings>();
                    settings.Should().NotBeNull();

                    settings.Height.Should().Be(20);
                    settings.Width.Should().Be(50);
                });
        }

        [Fact(DisplayName = nameof(SettingConfigSectionNotDefined_DefaultUnitilizedInstance))]
        public void SettingConfigSectionNotDefined_DefaultUnitilizedInstance()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.DefaultSettingsConfig();
                    config.AddPlugin<MockCorePlugin>().AddPluginType<MockSetttings>();
                })

                .Test(
                    c => c.Build(),
                    (IAppContainer c) =>
                    {
                        var settings = c.Services.Resolve<MockSetttings>();
                        settings.Should().NotBeNull();

                        settings.Height.Should().Be(1000);
                        settings.Width.Should().Be(2000);
                    });
        }

        [Fact (DisplayName = nameof(CanReadSpecificSettingValue_FromConfiguration))]
        public void CanReadSpecificSettingValue_FromConfiguration()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.DefaultSettingsConfig();
                    config.AddPlugin<MockCorePlugin>().AddPluginType<MockSetttings>();
                })

                .Test(c =>
                {
                    c.WithConfig<EnvironmentConfig>(settings => {

                        AddInMemorySettings(settings);
                    });

                    c.Build();
                },
                (IAppContainer c) =>
                {
                    var configuration = c.Services.Resolve<IConfiguration>();

                    int width = configuration.GetValue<int>("App:MainWindow:Width");
                    width.Should().Be(50);
                });
        }

        [Fact(DisplayName = nameof(WhenSettingsDerived_LoadsBaseAndDerivedProperties))]
        public void WhenSettingsDerived_LoadsBaseAndDerivedProperties()
        {
            ContainerSetup
                .Arrange((TestTypeResolver resolver) =>
                {
                    resolver.DefaultSettingsConfig();
                    resolver.AddPlugin<MockCorePlugin>().AddPluginType<MockDerivedSettings>();
                })

                .Test(c =>
                {
                    c.WithConfig<EnvironmentConfig>(settings => {

                        AddInMemorySettings(settings);
                    });

                    c.Build();
                },
                (IAppContainer c) =>
                {
                    var settings = c.Services.Resolve<MockDerivedSettings>();
                    settings.Should().NotBeNull();

                    settings.Height.Should().Be(20);
                    settings.Width.Should().Be(50);
                    settings.Dialog.Colors.Frame.Should().Be("RED");
                    settings.Dialog.Colors.Title.Should().Be("DARK_RED");
                });
        }

        private void AddInMemorySettings(EnvironmentConfig config)
        {
            var builder = new ConfigurationBuilder();

            var dict = new Dictionary<string, string>
                {
                    {"App:MainWindow:Height", "20"},
                    {"App:MainWindow:Width", "50"},
                    {"App:MainWindows:ValidatedValue", "3" },
                    {"App:MainWindow:Dialog:Colors:Frame", "RED"},
                    {"App:MainWindow:Dialog:Colors:Title", "DARK_RED"}
                };

            builder.AddInMemoryCollection(dict);

            config.UseConfiguration(builder.Build());
        }
    }
}

