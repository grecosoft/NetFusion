using CoreTests.Settings.Mocks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using System.Collections.Generic;
using Xunit;

namespace CoreTests.Settings
{
    /// <summary>
    /// Unit tests for testing the injecting of services directly into the dependent
    /// service.  The Settings plugin when bootstrapped, locates all classes implementing
    /// the IAppSetting interface and automatically configures them with the service
    /// collection. 
    /// </summary>
    public class SettingsTests
    {
        /// <summary>
        /// A configuration setting class deriving from IAppSetting and marked with the
        /// ConfigurationSection attribute will be loaded from the associated section
        /// and can be directly injected into a dependent service.
        /// </summary>
        [Fact(DisplayName = "Can inject Settings directly into Consumer")]
        public void CanInjectSettings_DirectlyIntoConsumer()
        {
            ContainerFixture.Test((System.Action<ContainerFixture>)(fixture => {
                fixture.Arrange
                    .Resolver((System.Action<TestTypeResolver>)(r =>
                    {
                        r.AddPluginsUnderTests();
                        r.AddPlugin<MockCorePlugin>().AddPluginType<MockSetttings>();
                    }))
                    .Configuration(c =>
                    {
                        AddInMemorySettings(c);
                    })
                    .Assert.Services2(s =>
                    {
                        var settings = s.GetService<MockSetttings>();
                        settings.Should().NotBeNull();

                        settings.Height.Should().Be(20);
                        settings.Width.Should().Be(50);
                    });
            }));
        }

        /// <summary>
        /// Settings class can be derived and each marked with the ConfigurationSetion attribute.
        /// When there are multiple setting classes defined within a hierarchy, the section names
        /// are appended from the parent to the child setting class being injected.  This combined
        /// section path is then used to locate the configuration section from which the settings
        /// should be populated.
        /// </summary>
        [Fact(DisplayName = "When settings derived loads base and derived properties")]
        public void WhenSettingsDerived_LoadsBaseAndDerivedProperties()
        {
            ContainerFixture.Test((System.Action<ContainerFixture>)(fixture => {
                fixture.Arrange
                    .Resolver((System.Action<TestTypeResolver>)(r =>
                    {
                        r.AddPluginsUnderTests();
                        r.AddPlugin<MockCorePlugin>().AddPluginType<MockDerivedSettings>();
                    }))
                    .Configuration(c =>
                    {
                        AddInMemorySettings(c);
                    })
                    .Assert.Services2(s =>
                    {
                        var settings = s.GetService<MockDerivedSettings>();
                        settings.Should().NotBeNull();

                        settings.Height.Should().Be(20);
                        settings.Width.Should().Be(50);
                        settings.Dialog.Colors.Frame.Should().Be("RED");
                        settings.Dialog.Colors.Title.Should().Be("DARK_RED");
                    });
            }));
        }

        /// <summary>
        /// The IConfiguration service is automatically registered with the service 
        /// collection when the AppContainer is built.  This can be used to load a
        /// specific value when there is not a needed from a typed settings class.
        /// </summary>
        [Fact (DisplayName = "Can read specific settings value from configuration")]
        public void CanReadSpecificSettingValue_FromConfiguration()
        {
            ContainerFixture.Test((System.Action<ContainerFixture>)(fixture => {
                fixture.Arrange
                    .Resolver((System.Action<TestTypeResolver>)(r =>
                    {
                        r.AddPluginsUnderTests();
                        r.AddPlugin<MockCorePlugin>();
                    }))
                    .Configuration(c =>
                    {
                        AddInMemorySettings(c);
                    })
                    .Assert.Services2(s =>
                    {
                        var configuration = s.GetService<IConfiguration>();

                        int width = configuration.GetValue<int>("App:MainWindow:Width");
                        width.Should().Be(50);
                    });
            }));
        }

        /// <summary>
        /// If needed, the settings can be obtained by a service by injecting the IOptions
        /// interface for the needed options type.  If needed this can be used, but injecting
        /// the settings directly in to the service is more direct and easier to unit-test
        /// the dependent service.
        /// </summary>
        [Fact(DisplayName = "Can read specific settings object using options")]
        public void CanReadSpecificSettings_UsingOptions()
        {
            ContainerFixture.Test((System.Action<ContainerFixture>)(fixture => {
                fixture.Arrange
                    .Resolver((System.Action<TestTypeResolver>)(r =>
                    {
                        r.AddPluginsUnderTests();
                        r.AddPlugin<MockCorePlugin>().AddPluginType<MockSetttings>();
                    }))
                    .Configuration(c =>
                    {
                        AddInMemorySettings(c);
                    })
                    .Assert.Services2(s =>
                    {
                        var options = s.GetService<IOptions<MockSetttings>>();
                        options.Should().NotBeNull();

                        var settings = options.Value;
                        settings.Should().NotBeNull();

                        settings.Height.Should().Be(20);
                        settings.Width.Should().Be(50);
                    });
            }));
        }

        // Test the generation of section for derived setting types.
        // Pass in type catalog and validate that settings are added to service collection.
        // Test validation by passing catalog and creating service provider and then reading setting.
        // Test logging of settings.

        private void AddInMemorySettings(IConfigurationBuilder builder)
        {
            var dict = new Dictionary<string, string>
                {
                    {"App:MainWindow:Height", "20"},
                    {"App:MainWindow:Width", "50"},
                    {"App:MainWindows:ValidatedValue", "3" },
                    {"App:MainWindow:Dialog:Colors:Frame", "RED"},
                    {"App:MainWindow:Dialog:Colors:Title", "DARK_RED"}
                };

            builder.AddInMemoryCollection(dict);
        }
    }
}

