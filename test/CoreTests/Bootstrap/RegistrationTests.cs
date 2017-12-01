using Autofac;
using CoreTests.Bootstrap.Mocks;
using FluentAssertions;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Extensions;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

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
        /// The host application can register components as services during the bootstrap process.  
        /// This will often be for services such as logging that will need to be created before the 
        /// bootstrap process.
        /// </summary>
        [Fact(DisplayName = "Host specified Services Registered")]
        public void HostSpecifiedServices_Registered()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>();
                })
                .Act.OnContainer(c =>
                {
                    c.WithConfig<AutofacRegistrationConfig>(rc =>
                    {
                        rc.Build = builder =>
                        {
                            builder.RegisterType<MockHostServiceType>()
                                .As<IMockHostServiceType>();
                        };
                    });
                    c.Build();
                })
                .Assert.Container(c =>
                {
                    var hostRegistration = c.Services.Resolve<IMockHostServiceType>();
                    hostRegistration.Should().NotBeNull();
                    hostRegistration.Should().BeOfType<MockHostServiceType>();
                });

            });
        }

        /// <summary>
        /// When a component implementing the IComponentActivated interface is registered,
        /// in the dependency injection container and invokes the NotifyOnActivating method, 
        /// the component's OnActivated method will be invoked.
        /// </summary>
        [Fact(DisplayName = "Component can be Notified on Creation")]
        public void Component_CanBeNotified_OnCreation()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>()
                     .AddPluginType<MockActivatedType>();
                })
                .Act.OnContainer(c =>
                {
                    c.WithConfig<AutofacRegistrationConfig>(rc =>
                    {
                        rc.Build = (bdr) => bdr.RegisterType<MockActivatedType>()
                            .NotifyOnActivating()
                            .AsSelf()
                            .SingleInstance();
                    });

                    c.Build();
                })
                .Assert.Container(c =>
                {
                    var comp = c.Services.Resolve<MockActivatedType>();
                    comp.Should().NotBeNull();
                    comp.IsActivated.Should().BeTrue();
                });

            });
        }
        
        /// <summary>
        /// The application container is also registered within the dependency
        /// injection container as a service.
        /// </summary>
        [Fact(DisplayName = "Application Container registered as Singleton Service")]
        public void AppContainer_Registered_AsSingletonService()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>();
                })
                .Act.OnContainer(c =>
                {
                    c.Build();
                })
                .Assert.Container(c =>
                {
                    var appContainer = c.Services.Resolve<IAppContainer>();
                    appContainer.Should().BeSameAs(c);
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
                fixture.Arrange.Resolver(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>()
                     .AddPluginType<MockPluginOneModule>();
                })
                .Act.OnContainer(c =>
                {
                    c.Build();
                })
                .Assert.Container(c =>
                {
                    var moduleAsService = c.Services.Resolve<IMockPluginOneModule>();
                    moduleAsService.Should().NotBeNull();
                    moduleAsService.Should().BeOfType<MockPluginOneModule>();
                });

            });
        }
    }
}
