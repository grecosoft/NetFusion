using FluentAssertions;
using NetFusion.Bootstrap.Container;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using CoreTests.Bootstrap.Mocks;

namespace CoreTests.Bootstrap
{
    /// <summary>
    /// Modules when called during the bootstrap process can register components with
    /// the dependency injection container.  In addition, specific convention based
    /// components will also be registered.  the application host can also specify
    /// components to be registered with the DI container.
    /// </summary>
    public class RegistrationTests
    {
        /// <summary>
        /// The application container is also registered within the dependency
        /// injection container as a service.
        /// </summary>
        [Fact(DisplayName = "Application Container registered as Singleton Service")]
        public void AppContainer_Registered_AsSingletonService()
        {
            ContainerFixture.Test((System.Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((System.Action<TestTypeResolver>)(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>();
                }))
                .Assert.Services2(s =>
                {
                    var appContainer = s.GetService<IAppContainer>();
                    appContainer.Should().NotBeNull();
                });

            }));
        }

        /// <summary>
        /// Plug-in modules implementing any interfaces deriving from the base
        /// IPluginModuleService interface will automatically be registered as
        /// a service within the dependency-injection container.
        /// </summary>
        [Fact(DisplayName = "Module Registered as Service if implements Marker interface")]
        public void ModuleRegisteredAsService_IfImplements_MarkerInterface()
        {
            ContainerFixture.Test((System.Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((System.Action<TestTypeResolver>)(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>()
                     .AddPluginType<MockPluginOneModule>();
                }))
                .Assert.Services2(s =>
                {
                    var moduleAsService = s.GetService<IMockPluginOneModule>();
                    moduleAsService.Should().NotBeNull();
                    moduleAsService.Should().BeOfType<MockPluginOneModule>();
                });

            }));
        }
    }
}
