using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetFusion.Common.Base.Validation;
using NetFusion.Core.Bootstrap.Catalog;
using NetFusion.Core.Settings;
using NetFusion.Core.Settings.Plugin.Modules;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Core.UnitTests.Settings.Setup;

namespace NetFusion.Core.UnitTests.Settings;

public class SettingBootstrapTests
{
    /// <summary>
    /// When normally adding configuration settings as option classes for a section, one needs to invoke
    /// Configure on the ServiceCollection for each option and specify the associated configuration path.
    /// The NetFusion.Plugin does this automatically during the bootstrap process by locating all classes
    /// implementing IAppSettings and automatically configuring them using the section path determined
    /// by the specified ConfigurationSection attributes.
    /// </summary>
    [Fact]
    public void AllAppSettings_Registered_AsOptions()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange
                .Container(c => TestSetup.AddSettingsPlugin(c, typeof(TestSettingsOne), typeof(TestSettingsTwo)))
                .Assert.ServiceCollection(sc =>
                {
                    var serviceTypes = sc.Select(s => s.ServiceType).ToArray();
                        
                    Assert.Contains(typeof(IOptionsChangeTokenSource<TestSettingsOne>), serviceTypes);
                    Assert.Contains(typeof(IConfigureOptions<TestSettingsOne>), serviceTypes);
                    Assert.Contains(typeof(IOptionsChangeTokenSource<TestSettingsTwo>), serviceTypes);
                    Assert.Contains(typeof(IConfigureOptions<TestSettingsTwo>), serviceTypes);
                });
        });            
    }

    /// <summary>
    /// To make the use of configuration options easier, the settings plug-in module also
    /// registers each IAppSetting class within the service collection as a factory method
    /// that, when invoked, will return the settings option.
    /// </summary>
    [Fact]
    public void AllAppSettings_Registered_AsFactory()
    {
        // Arrange:
        var catalog = new TypeCatalog(new ServiceCollection(), typeof(TestSettingsOne));
        var module = new AppSettingsModule();
            
        // Act:
        module.ScanForServices(catalog);
            
        // Assert:
        Assert.Single(catalog.Services);
        Assert.NotNull(catalog.Services.First().ImplementationFactory);
        Assert.Equal(ServiceLifetime.Singleton, catalog.Services.First().Lifetime);
    }

    /// <summary>
    /// All IAppSetting derived concrete class can be directly injected into a dependent
    /// component without have to used IOptions.  A factory method is associated with
    /// each IAppSetting derived type that automatically obtains the option and returns
    /// the value.
    /// </summary>
    [Fact]
    public void DependentComponent_HasSettingsInjected()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange
                .Configuration(config =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "A:B:Value1", "500" },
                        { "A:B:Value2", "600" }
                    });
                })
                .Services(s =>
                {
                    // Add service with settings dependency:
                    s.AddSingleton<DependentComponent>();
                })
                .Container(c => TestSetup.AddSettingsPlugin(c, typeof(TestSettingsOne)))
                .Assert.Services(s =>
                {
                    var component = s.GetRequiredService<DependentComponent>();
            
                    Assert.NotNull(component.InjectedSettings);
                    Assert.Equal(500, component.InjectedSettings.Value1);
                    Assert.Equal(600, component.InjectedSettings.Value2);
                });
        });            
    }

    /// <summary>
    /// If derived IAppSetting class also implements the IValidatableType interface,
    /// the state is validated before being injected into the dependent component.
    /// </summary>
    [Fact]
    public void WhenAppSetting_Injected_StateValidated()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange
                .Configuration(config =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "A:B:Value1", "900" },
                        { "A:B:Value2", "600" }
                    });
                })
                .Services(s =>
                {
                    // Add service with settings dependency:
                    s.AddSingleton<DependentComponentInvalid>();
                })
                .Container(c => TestSetup.AddSettingsPlugin(c, typeof(TestSettingsInvalid)))
                .Act.RecordException().OnServices(s =>
                {
                    s.GetRequiredService<DependentComponentInvalid>();
                })
                .Assert.Exception(ex =>
                {
                    ex.Should().BeOfType<SettingsValidationException>();
                        
                    var valEx = (SettingsValidationException) ex;
                    valEx.SettingsSection.Should().Be("A:B");
                    valEx.SettingsType.Should().Be(typeof(TestSettingsInvalid));
                    valEx.ValidationResults.Should().NotBeNull();

                    var validations = valEx.ValidationResults.ObjectValidations
                        .SelectMany(ov => ov.Validations).ToArray();

                    validations.Should().HaveCount(1);
                    validations.First().Message.Should().Be("Value1 must be greater than Value2");
                });
        });           
    }

    [ConfigurationSection("A:B")]
    public class TestSettingsOne: IAppSettings
    {
        public int Value1 { get; set; } = 10;
        public int Value2 { get; set; } = 100;
    }
        
    [ConfigurationSection("A:B")]
    public class TestSettingsInvalid: IAppSettings,
        IValidatableType
    {
        public int Value1 { get; set; } = 10;
        public int Value2 { get; set; } = 100;
            
        public void Validate(IObjectValidator validator)
        {
            validator.Verify(Value1 <= Value2, "Value1 must be greater than Value2");
        }
    }

    [ConfigurationSection("X:Y")]
    public class TestSettingsTwo : IAppSettings;

    public class DependentComponent
    {
        public TestSettingsOne InjectedSettings { get; }
            
        public DependentComponent(TestSettingsOne settings)
        {
            InjectedSettings = settings;
        }
    }
        
    public class DependentComponentInvalid
    {
        public TestSettingsInvalid InjectedSettings { get; }
            
        public DependentComponentInvalid(TestSettingsInvalid settings)
        {
            InjectedSettings = settings;
        }
    }
}