using Autofac;
using FluentAssertions;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Extensions;
using NetFusion.Bootstrap.Testing;
using NetFusion.Tests.Core.Bootstrap.Mocks;
using Xunit;

namespace NetFusion.Tests.Core.Bootstrap
{
    public class RegistrationUnitTests
    {
        /// <summary>
        /// The application container is also registered within the dependency
        /// injection container as a service.
        /// </summary>
        [Fact]
        public void AppContainerRegisteredAsSingletonService()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>();
                })
                .Act(c => c.Build())
                .Assert((AppContainer ac) =>
                {
                    var appContainer = ac.Services.Resolve<IAppContainer>();
                    appContainer.Should().BeSameAs(ac);
                });
        }

        /// <summary>
        /// Plug-in modules implementing any interfaces deriving from the base
        /// IPluginModuleService interface will automatically be registered as
        /// a service within the dependency-injection container.
        /// </summary>
        [Fact]
        public void ModuleRegisteredAsService_IfImplementsMarkerInterface()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockPluginOneModule>();
                })
                .Act(c => c.Build())
                .Assert((AppContainer ac) =>
                {
                    var moduleAsService = ac.Services.Resolve<IMockPluginOneModule>();
                    moduleAsService.Should().NotBeNull();
                    moduleAsService.Should().BeOfType<MockPluginOneModule>();
                });
        }

        /// <summary>
        /// The host application can also register components as services during
        /// the bootstrap process.  This will often be for services such as logging
        /// that will need to be created before the bootstrap process.
        /// </summary>
        [Fact]
        public void HostSpecifiedServicesRegistered()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>();
                })
                .Act(c =>
                {
                    c.WithConfig<AutofacRegistrationConfig>(rc =>
                    {
                        rc.Build = builder =>
                        {
                            builder.RegisterType<MockHostType>()
                                .As<IMockHostType>();
                        };
                    });
                    c.Build();
                })
                .Assert((AppContainer c) =>
                {
                    var hostRegistration = c.Services.Resolve<IMockHostType>();
                    hostRegistration.Should().NotBeNull();
                    hostRegistration.Should().BeOfType<MockHostType>();
                });
        }

        /// <summary>
        /// When a component implementing the IComponentActivated interface is registered 
        /// in the dependency injection container and invokes the NotifyOnActivating 
        /// method, the component's OnActivated method will be invoked.
        /// </summary>
        [Fact]
        public void ComponentCanBeActivedOnCreation()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockActivatedType>();
                })
                .Act(c =>
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
                .Assert((AppContainer c) =>
                {
                    var comp = c.Services.Resolve<MockActivatedType>();
                    comp.Should().NotBeNull();
                    comp.IsActivated.Should().BeTrue();
                });
        }
    }
}
