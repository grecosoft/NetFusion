using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetFusion.Core.Settings;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Core.UnitTests.Settings.Mocks;
using NetFusion.Core.UnitTests.Settings.Setup;

namespace NetFusion.Core.UnitTests.Settings;

/// <summary>
/// Unit tests for testing the injecting of services directly into the dependent
/// service.  The Settings plugin when bootstrapped, locates all classes implementing
/// the IAppSetting interface and automatically configures them with the service
/// collection. 
/// </summary>
public class SettingTests
{
    /// <summary>
    /// A configuration setting class deriving from IAppSetting and marked with the
    /// ConfigurationSection attribute will be loaded from the associated section
    /// and can be directly injected into a dependent service.
    /// </summary>
    [Fact(DisplayName = "Can inject Settings directly into Consumer")]
    public void CanInjectSettings_DirectlyIntoConsumer()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange
                .Configuration(TestSetup.AddInMemorySettings)
                .Container(c => TestSetup.AddSettingsPlugin(c, typeof(MockSettings)))
                .Assert.Services(s =>
                {
                    var settings = s.GetService<MockSettings>();
                    settings.Should().NotBeNull();

                    settings.Height.Should().Be(20);
                    settings.Width.Should().Be(50);
                });
        });
    }

    /// <summary>
    /// The IConfiguration service is automatically registered with the service 
    /// collection when the AppContainer is built.  This can be used to load a
    /// specific value when there is not a needed from a typed settings class.
    /// </summary>
    [Fact (DisplayName = "Can read specific settings value from configuration")]
    public void CanReadSpecificSettingValue_FromConfiguration()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange
                .Configuration(TestSetup.AddInMemorySettings)
                .Container(c => TestSetup.AddSettingsPlugin(c, typeof(MockSettings)))
                .Assert.Services(s =>
                {
                    var configuration = s.GetService<IConfigurationRoot>();

                    int width = configuration.GetValue<int>("App:MainWindow:Width");
                    width.Should().Be(50);
                });
        });
    }

    [Fact]
    public void CanReadSettingsDirectlyFromConfiguration()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange
                .Configuration(TestSetup.AddInMemorySettings)
                .Container(c => TestSetup.AddSettingsPlugin(c, typeof(MockSettings)))
                .Assert.Services(s =>
                {
                    var configuration = s.GetService<IConfigurationRoot>();

                    var settings = configuration.GetSettings<MockSettings>();
                    settings.Should().NotBeNull();
                });
        });
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
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange
                .Configuration(TestSetup.AddInMemorySettings)
                .Container(c => TestSetup.AddSettingsPlugin(c, typeof(MockSettings)))
                .Assert.Services(s =>
                {
                    var options = s.GetService<IOptions<MockSettings>>();
                    options.Should().NotBeNull();

                    var settings = options.Value;
                    settings.Should().NotBeNull();

                    settings.Height.Should().Be(20);
                    settings.Width.Should().Be(50);
                });
        });
    }
}