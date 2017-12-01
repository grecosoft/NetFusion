using Autofac;
using BootstrapTests.Settings.Mocks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NetFusion.Bootstrap.Configuration;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using System.Collections.Generic;
using Xunit;

namespace CoreTests.Settings
{
    /// <summary>
    /// Unit tests for testing the injecting of configuration setting classes
    /// that are loaded from the providers specified on the ConfigurationBuilder.
    /// </summary>
    public class SettingsTests
    {
        /// <summary>
        /// A configuration setting class deriving from IAppSetting and marked with the
        /// ConfigurationSection attribute will be loaded from the associated section.
        /// </summary>
        [Fact(DisplayName = "Can inject Settings directly into Consumer")]
        public void CanInjectSettings_DirectlyIntoConsumer()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange.Resolver(r =>
                {
                    r.DefaultSettingsConfig();
                    r.AddPlugin<MockCorePlugin>().AddPluginType<MockSetttings>();
                })
                .Act.OnContainer(c =>
                {
                    c.WithConfig<EnvironmentConfig>(settings => {

                        AddInMemorySettings(settings);
                    });

                    c.Build();
                })
                .Assert.Container(c =>
                {
                    var settings = c.Services.Resolve<MockSetttings>();
                    settings.Should().NotBeNull();

                    settings.Height.Should().Be(20);
                    settings.Width.Should().Be(50);
                });
            });
        }

        [Fact(DisplayName = "Settings config section Not defined Default uninitialized Instance returned")]
        public void SettingConfigSectionNotDefined_DefaultUnintializedInstanceReturned()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange.Resolver(r =>
                {
                    r.DefaultSettingsConfig();
                    r.AddPlugin<MockCorePlugin>().AddPluginType<MockSetttings>();
                })
                .Act.OnContainer(c =>
                {
                    c.Build();
                })
                .Assert.Container(c =>
                {
                    var settings = c.Services.Resolve<MockSetttings>();
                    settings.Should().NotBeNull();

                    settings.Height.Should().Be(1000);
                    settings.Width.Should().Be(2000);
                });
            });
        }

        [Fact (DisplayName = "Can read specific settings value from configuration")]
        public void CanReadSpecificSettingValue_FromConfiguration()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange.Resolver(r =>
                {
                    r.DefaultSettingsConfig();
                    r.AddPlugin<MockCorePlugin>().AddPluginType<MockSetttings>();
                })
                .Act.OnContainer(c =>
                {
                    c.WithConfig<EnvironmentConfig>(settings => {

                        AddInMemorySettings(settings);
                    });

                    c.Build();
                })
                .Assert.Container(c =>
                {
                    var configuration = c.Services.Resolve<IConfiguration>();

                    int width = configuration.GetValue<int>("App:MainWindow:Width");
                    width.Should().Be(50);
                });
            });
        }

        [Fact(DisplayName = "When settings derived loads base and derived properties")]
        public void WhenSettingsDerived_LoadsBaseAndDerivedProperties()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange.Resolver(r =>
                {
                    r.DefaultSettingsConfig();
                    r.AddPlugin<MockCorePlugin>().AddPluginType<MockDerivedSettings>();
                })
                .Act.OnContainer(c =>
                {
                    c.WithConfig<EnvironmentConfig>(settings => {

                        AddInMemorySettings(settings);
                    });

                    c.Build();
                })
                .Assert.Container(c =>
                {
                    var settings = c.Services.Resolve<MockDerivedSettings>();
                    settings.Should().NotBeNull();

                    settings.Height.Should().Be(20);
                    settings.Width.Should().Be(50);
                    settings.Dialog.Colors.Frame.Should().Be("RED");
                    settings.Dialog.Colors.Title.Should().Be("DARK_RED");
                });
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

