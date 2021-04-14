using FluentAssertions;
using NetFusion.Test.Container;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using CoreTests.Bootstrap.Mocks;
using NetFusion.Bootstrap.Container;
using NetFusion.Test.Plugins;

namespace CoreTests.Bootstrap
{
    /// <summary>
    /// Modules when called during the bootstrap process, can register components with
    /// the dependency injection container.  In addition, specific convention based
    /// components will also be registered. 
    /// </summary>
    public class RegistrationTests
    {
        /// <summary>
        /// The application container is also registered within the dependency
        /// injection container as a service.
        /// </summary>
        [Fact(DisplayName = "Application Container registered as Singleton Service")]
        public void CompositeApplication_Registered_AsSingletonService()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                    })
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
        [Fact(DisplayName = "Module Registered as Service if implements Marker interface")]
        public void ModuleRegisteredAsService_IfImplements_MarkerInterface()
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
    }
}

