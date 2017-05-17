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
    public class RegistrationTests
    {
        /// <summary>
        /// The application container is also registered within the dependency
        /// injection container as a service.
        /// </summary>
        [Fact(DisplayName = nameof(AppContainerRegistered_AsSingletonService))]
        public void AppContainerRegistered_AsSingletonService()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>();
                })
                .Test(
                c => c.Build(), 
                (IAppContainer ac) =>
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
        [Fact(DisplayName = nameof(ModuleRegisteredAsService_IfImplementsMarkerInterface))]
        public void ModuleRegisteredAsService_IfImplementsMarkerInterface()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockPluginOneModule>();
                })
                .Test(
                    c => c.Build(),
                    (IAppContainer ac) =>
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
        [Fact(DisplayName = nameof(HostSpecifiedServices_Registered))]
        public void HostSpecifiedServices_Registered()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>();
                })
                .Test(c =>
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
                }, 
                (IAppContainer c) =>
                {
                    var hostRegistration = c.Services.Resolve<IMockHostServiceType>();
                    hostRegistration.Should().NotBeNull();
                    hostRegistration.Should().BeOfType<MockHostServiceType>();
                });
        }

        /// <summary>
        /// When a component implementing the IComponentActivated interface is registered 
        /// in the dependency injection container and invokes the NotifyOnActivating 
        /// method, the component's OnActivated method will be invoked.
        /// </summary>
        [Fact(DisplayName = nameof(ComponentCanBeNotified_OnCreation))]
        public void ComponentCanBeNotified_OnCreation()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockActivatedType>();
                })
                .Test(c =>
                {
                    c.WithConfig<AutofacRegistrationConfig>(rc =>
                    {
                        rc.Build = (bdr) => bdr.RegisterType<MockActivatedType>()
                            .NotifyOnActivating()
                            .AsSelf()
                            .SingleInstance();
                    });

                    c.Build();
                },
                (IAppContainer c) =>
                {
                    var comp = c.Services.Resolve<MockActivatedType>();
                    comp.Should().NotBeNull();
                    comp.IsActivated.Should().BeTrue();
                });
        }

    }
}
