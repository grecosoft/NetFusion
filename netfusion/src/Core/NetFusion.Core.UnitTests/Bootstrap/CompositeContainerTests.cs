using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Base.Logging;
using NetFusion.Common.Base.Scripting;
using NetFusion.Common.Base.Validation;
using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Core.UnitTests.Bootstrap.Mocks;

namespace NetFusion.Core.UnitTests.Bootstrap;

/// <summary>
/// Modules when called during the bootstrap process, can register components with
/// the dependency injection container.  In addition, specific convention based
/// components will also be registered. 
/// </summary>
public class CompositeContainerTests
{
    /// <summary>
    /// The application container is also registered within the dependency
    /// injection container as a service.
    /// </summary>
    [Fact]
    public void CompositeApplication_Registered_AsSingletonService()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.Container(c => { c.RegisterPlugin<MockHostPlugin>(); })
                .Assert.Services(s =>
                {
                    var compositeApp = s.GetService<ICompositeApp>();
                    compositeApp.Should().NotBeNull();
                });
        });
    }

    /// <summary>
    /// Plug-in modules implementing any interfaces deriving from the base
    /// IPluginModuleService interface will automatically be registered as
    /// a service within the dependency-injection container.
    /// </summary>
    [Fact]
    public void ModuleRegisteredAsService_Implements_MarkerInterface()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.Container(c =>
                {
                    var testPlugin = new MockHostPlugin();
                    testPlugin.AddModule<MockPluginOneModule>();

                    c.RegisterPlugins(testPlugin);
                })
                .Assert.Services(s =>
                {
                    var moduleAsService = s.GetService<IMockPluginOneModule>();
                    moduleAsService.Should().NotBeNull();
                    moduleAsService.Should().BeOfType<MockPluginOneModule>();
                });
        });
    }

    /// <summary>
    /// The CompositeContainerBuilder populates Microsoft's IServiceCollection with
    /// service components.  A reference to the CompositeContainerBuilder is obtained
    /// by calling an IServiceCollection extension method.
    /// </summary>
    [Fact]
    public void CompositeContainerBuilder_CreatedFrom_ServiceCollection()
    {
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().Build();
            
        services.AddLogging();
            
        var builder = services.CompositeContainer(configuration, new NullExtendedLogger());
        var hostPlugin = new MockHostPlugin();
        var corePlugin = new MockCorePlugin();

        builder.AddPlugin(hostPlugin, corePlugin);
        builder.Compose();

        var provider = services.BuildServiceProvider();
        var compositeApp = provider.GetRequiredService<ICompositeApp>();

        compositeApp.Should().NotBeNull();
        CompositeApp.Instance.Should().NotBeNull();
    }

    /// <summary>
    /// The host can override plugin specific settings by calling the InitPluginConfig
    /// generic method passing the configuration type to be updated.
    /// </summary>
    [Fact]
    public void CompositeContainerBuilder_UsedTo_SetPluginConfigurations()
    {
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().Build();
            
        services.AddLogging();
            
        var builder = services.CompositeContainer(configuration, new NullExtendedLogger());
        var hostPlugin = new MockHostPlugin();
        var corePlugin = new MockCorePlugin();
            
        corePlugin.AddConfig<MockPluginConfigOne>();
        builder.AddPlugin(hostPlugin, corePlugin);

        builder.InitPluginConfig<MockPluginConfigOne>(c => c.ConfigValue = "10000");
        corePlugin.GetConfig<MockPluginConfigOne>().ConfigValue.Should().Be("10000");
            
        builder.Compose();
    }

    /// <summary>
    /// Certain global services such as serialization have default implementations
    /// registered.
    /// </summary>
    [Fact]
    public void CompositeContainerBuilder_Registers_DefaultServices()
    {
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().Build();

        var builder = services.CompositeContainer(configuration, new NullExtendedLogger());
        var hostPlugin = new MockHostPlugin();

        builder.AddPlugin(hostPlugin);
        builder.Compose();

        var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IValidationService>();
    }
}