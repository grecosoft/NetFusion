using FluentAssertions;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Testing;
using NetFusion.Core.Tests.Bootstrap.Mocks;
using System.Linq;
using Xunit;

namespace NetFusion.Tests.Bootstrap
{
    /// <summary>
    /// Unit-tests for validating the overall application container implementation.  
    /// The AppContainer class is responsible for locating all plug-in assemblies,
    /// validating their configuration, loading and  composing their modules, and 
    /// building a resulting dependency-injection container that is used by the 
    /// running application to consume services.
    /// </summary>
    public class ContainerTests
    {
        /// <summary>
        /// After the application container is created, a singleton instance can be referenced.
        /// </summary>
        [Fact]
        public void CreatedAppContainerCanBeReferenced()
        {
            ContainerSetup
                .Arrange((HostTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>();
                })
                .Act(c => c.Build())
                .Assert(() =>
                {
                    AppContainer.Instance.Should().NotBeNull();
                });
        }

        /// <summary>
        /// The application container can only be created and bootstrapped once.
        /// </summary>
        [Fact]
        public void ContainerCanOnlyBeCreatedOnce()
        {
            Assert.Throws<ContainerException>(() =>
            {
                AppContainer.Create(new[] { "*.dll", "*.exe" });
                AppContainer.Create(new[] { "*.dll", "*.exe" });
            });
        }

        /// <summary>
        /// When the application container is disposed, each plug-in module
        /// will be disposed.
        /// </summary>
        [Fact]
        public void WhenAppContainerDisposed_PluginModulesDisposed()
        {
            ContainerSetup
               .Arrange((HostTypeResolver config) =>
               {
                   config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockPluginOneModule>();
               })
               .Act(c => { c.Build(); c.Dispose(); })
               .Assert((CompositeApplication ca) =>
               {
                   var pluginModule = ca.AppHostPlugin.PluginModules.First();
                   (pluginModule as MockPluginModule).IsDisposed.Should().BeTrue();
               });
        }

        /// <summary>
        /// After the application-container is built, the registered 
        /// services can be accessed.
        /// </summary>
        [Fact]
        public void AfterContainerBuilt_ServicesAvailable()
        {
            ContainerSetup
               .Arrange((HostTypeResolver config) =>
               {
                   config.AddPlugin<MockAppHostPlugin>();
               })
               .Act(c => { c.Build(); })
               .Assert((AppContainer c) =>
               {
                   c.Services.Should().NotBeNull();
               });
        }

        /// <summary>
        /// Once the application container is disposed, the its provided
        /// services can no longer be accessed.
        /// </summary>
        [Fact]
        public void AppContainerDisposed_CannotAccessServices()
        {
            ContainerSetup
               .Arrange((HostTypeResolver config) =>
               {
                   config.AddPlugin<MockAppHostPlugin>();
               })
               .Act(c =>
               {
                   c.Build();
                   c.Dispose();

                   var s = c.Services;
               })
               .Assert((c, e) =>
               {
                   e.Should().BeOfType<ContainerException>();
               });
        }

        /// <summary>
        /// After the application container is built and started, each plug-in
        /// modules is given an opportunity to start.  This is where each 
        /// module is allowed to initialize any runtime services.  For example,
        /// this is where a service bus plug-in would create the needed queues
        /// and subscribe consumers.
        /// </summary>
        [Fact]
        public void EachPluginModuleStarted()
        {
            ContainerSetup
                .Arrange((HostTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockPluginOneModule>();

                    config.AddPlugin<MockCorePlugin>()
                        .AddPluginType<MockPluginTwoModule>();
                })
                .Act(c => c.Build().Start())
                .Assert((CompositeApplication ca) =>
                {
                    ca.AllPluginModules.OfType<MockPluginModule>()
                     .All(m => m.IsStarted).Should().BeTrue();
                });
        }

        /// <summary>
        /// When plug-in modules are initialized during the bootstrap process, they can scan other 
        /// plug-ins for concrete types deriving from base types it defines.  These base types are 
        /// referred to as a plug-in's know types.  When a plug-in type is based on a type defined
        /// within another plug-in, it is marked as being a discovered plug-in type.  This is how
        /// one plug-in integrates the services provided by another without having to know the 
        /// details.  This information is also used during logging.
        /// </summary>
        [Fact]
        public void PluginDiscoveredKnownTypesIdentified()
        {
            ContainerSetup
               .Arrange((HostTypeResolver config) =>
               {
                   // Plug-in type based on the type in the core plug-in.
                   config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockTypeOneBasedOnKnownType>();

                   // A type that the core plug-in knows about.
                   config.AddPlugin<MockCorePlugin>()
                        .AddPluginType<MockKnownType>() 
                        .AddPluginType<MockComposedModule>();
  
               })
               .Act(c => c.Build())
               .Assert((CompositeApplication ca) =>
               {
                   var discoveredTypes = ca.AppHostPlugin.PluginTypes.Where(pt => pt.DiscoveredByPlugins.Any());
                   discoveredTypes.Should().HaveCount(1);
                   discoveredTypes.First().Type.Should().Be(typeof(MockTypeOneBasedOnKnownType));
               });
        }

        /// <summary>
        /// Each plug-in known type that is discovered by a plug-in module has its 
        /// DiscoveredByPlugins property populated.  This indicates what plug-in
        /// modules discovered the type and had part in its initialization.
        /// </summary>
        [Fact]
        public void DiscoveredPluginsIdentified()
        {
            ContainerSetup
              .Arrange((HostTypeResolver config) =>
              {
                  // Plug-in type based on the type in the core plug-in.
                  config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockTypeOneBasedOnKnownType>();

                  // Core plug-in containing a module that is composed from type
                  // instances declared within another plug-in. 
                  config.AddPlugin<MockCorePlugin>()
                        .AddPluginType<MockComposedModule>()
                        .AddPluginType<MockKnownType>();
              })
              .Act(c => { c.Build(); c.Start(); })
              .Assert((CompositeApplication ca) =>
              {
                  var knownTypes = ca.AppHostPlugin.PluginTypes.Where(pt => pt.IsKnownType);
                  knownTypes.First().DiscoveredByPlugins.Should().HaveCount(1);
                  knownTypes.First().DiscoveredByPlugins.First().Manifest.Should().BeOfType<MockCorePlugin>();

              });
        }

        /// <summary>
        /// A module indicates that it wants to discover all concrete instances of a given 
        /// IKnownPluginType by declaring an enumerable property of that know-type.  The 
        /// property will be populated with instances that are derived for the specified
        /// known type.
        /// </summary>
        [Fact]
        public void ModuleKnownTypePropertiesPopulated()
        {
            ContainerSetup
              .Arrange((HostTypeResolver config) =>
              {
                  // Plug-in type based on the type in the core plug-in.
                  config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockTypeOneBasedOnKnownType>();

                  // Core plug-in containing a module that is composed from type
                  // instances declared within another plug-in. 
                  config.AddPlugin<MockCorePlugin>()
                        .AddPluginType<MockComposedModule>()
                        .AddPluginType<MockKnownType>();
              })
              .Act(c => { c.Build(); c.Start(); })
              .Assert((MockComposedModule cm) => {
                  cm.ImportedTypes.Should().NotBeNull();
                  cm.ImportedTypes.Should().HaveCount(1);
                  cm.ImportedTypes.First().Should().BeOfType<MockTypeOneBasedOnKnownType>();
              });
        }

        /// <summary>
        /// Plug-in IDs must be unique.  Plug-ins can use these key values to identity information
        /// related to a plug-in.  For example, the NetFusion.Settings.MongoDb plug-in uses the 
        /// plug-in ID of the application host when loading settings from a MongoDB collection.
        /// </summary>
        [Fact]
        public void PluginIdMustBeUnique()
        {
            ContainerSetup
                .Arrange((HostTypeResolver config) =>
                {
                    var appHostPlugin = new MockAppHostPlugin { PluginId = "1" };
                    var corePlugin = new MockCorePlugin { PluginId = "1" };

                    config.AddPlugin(appHostPlugin, corePlugin);
                })
                .Act(c => c.Build())
                .Assert((c, e) =>
                {
                    e.Should().NotBeNull();
                    e.Should().BeOfType<ContainerException>();
                });
        }

        /// <summary>
        /// Each plug-in has a name that is used when logging the
        /// container structure.
        /// </summary>
        [Fact]
        public void PluginNameMustBeSpecified()
        {
            ContainerSetup
                .Arrange((HostTypeResolver config) =>
                {
                    var appHostPlugin = new MockAppHostPlugin { Name = "" };
                    config.AddPlugin(appHostPlugin);
                })
                .Act(c => c.Build())
                .Assert((c, e) =>
                {
                    e.Should().NotBeNull();
                    e.Should().BeOfType<ContainerException>();
                });
        }

        /// <summary>
        /// While every .NET type has an assembly, the only code in the design that
        /// has a dependency on the actual .NET assembly is the classes that implement
        /// the ITypeResolver interface.  The type resolver specifies the name of the
        /// type's assembly which must be unique among plug-ins.
        /// </summary>
        [Fact]
        public void PluginAssemblyNameMustBeSpecified()
        {
            ContainerSetup
               .Arrange((HostTypeResolver config) =>
               {
                   var appHostPlugin = new MockAppHostPlugin { AssemblyName = "" };
                   config.AddPlugin(appHostPlugin);
               })
               .Act(c => c.Build())
               .Assert((c, e) =>
               {
                   e.Should().NotBeNull();
                   e.Should().BeOfType<ContainerException>();
               });
        }
    }

}
