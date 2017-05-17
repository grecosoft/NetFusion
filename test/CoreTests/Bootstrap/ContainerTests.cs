using CoreTests.Bootstrap.Mocks;
using FluentAssertions;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using System.Linq;
using Xunit;

namespace CoreTests.Bootstrap
{
    public class ContainerTests
    {
        /// <summary>
        /// When the application container is disposed, each plug-in module
        /// will be disposed.
        /// </summary>
        [Fact(DisplayName = nameof(AppContainerDisposed_PluginModulesAreDisposed))]
        public void AppContainerDisposed_PluginModulesAreDisposed()
        {
            ContainerSetup
               .Arrange((TestTypeResolver config) =>
               {
                   config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockPluginOneModule>();
               })
               .Test(
                    c => { c.Build(); c.Dispose(); }, 
                    (CompositeApplication ca) =>
                    {
                        var pluginModule = ca.AppHostPlugin.PluginModules.First();
                        (pluginModule as MockPluginModule).IsDisposed.Should().BeTrue();
                    });
        }

        /// <summary>
        /// Once the application container is disposed, the its provided
        /// services can no longer be accessed.
        /// </summary>
        [Fact(DisplayName = nameof(DisposedAppContainer_CannotHaveServicesAccessed))]
        public void DisposedAppContainer_CannotHaveServicesAccessed()
        {
            ContainerSetup
               .Arrange((TestTypeResolver config) =>
               {
                   config.AddPlugin<MockAppHostPlugin>();
               })
               .Test(c =>
               {
                   c.Build();
                   c.Dispose();

                   var s = c.Services;
               },
               (c, e) =>
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
        [Fact(DisplayName = nameof(WhenAppContainerStarted_EachPluginModuleStarted))]
        public void WhenAppContainerStarted_EachPluginModuleStarted()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockPluginOneModule>();

                    config.AddPlugin<MockCorePlugin>()
                        .AddPluginType<MockPluginTwoModule>();
                })
                .Test(
                    c => c.Build().Start(),
                    (CompositeApplication ca) =>
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
        [Fact(DisplayName = nameof(PluginDiscoveredKnownTypes_Identified))]
        public void PluginDiscoveredKnownTypes_Identified()
        {
            ContainerSetup
               .Arrange((TestTypeResolver config) =>
               {
                   // Plug-in type based on the type in the core plug-in.
                   config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockTypeOneBasedOnKnownType>();

                   // A type that the core plug-in knows about.
                   config.AddPlugin<MockCorePlugin>()
                        .AddPluginType<MockKnownType>()
                        .AddPluginType<MockComposedModule>();

               })
               .Test(
                    c => c.Build(), 
                    (CompositeApplication ca) =>
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
        [Fact(DisplayName = nameof(KnowTypesHave_DiscoveringPluginIdentified))]
        public void KnowTypesHave_DiscoveringPluginIdentified()
        {
            ContainerSetup
              .Arrange((TestTypeResolver config) =>
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
              .Test(
                    c =>  c.Build().Start(),
                    (CompositeApplication ca) =>
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
        [Fact(DisplayName = nameof(ModulesHavingKnownTypeProperties_WillBePopulated))]
        public void ModulesHavingKnownTypeProperties_WillBePopulated()
        {
            ContainerSetup
              .Arrange((TestTypeResolver config) =>
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
              .Test(
                c => c.Build().Start(), 
                (MockComposedModule cm) => {
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
        [Fact(DisplayName = nameof(PluginIdsValues_MustBeUnique))]
        public void PluginIdsValues_MustBeUnique()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    var appHostPlugin = new MockAppHostPlugin { PluginId = "1" };
                    var corePlugin = new MockCorePlugin { PluginId = "1" };

                    config.AddPlugin(appHostPlugin, corePlugin);
                })
                .Test(
                    c => c.Build(),
                    (c, e) =>
                    {
                        e.Should().NotBeNull();
                        e.Should().BeOfType<ContainerException>();
                    });
        }

        /// <summary>
        /// Each plug-in has a name that is used when logging the
        /// container structure.
        /// </summary>
        [Fact(DisplayName = nameof(PluginName_MustBeSpecified))]
        public void PluginName_MustBeSpecified()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    var appHostPlugin = new MockAppHostPlugin { Name = "" };
                    config.AddPlugin(appHostPlugin);
                })
                .Test(
                    c => c.Build(), 
                    (c, e) =>
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
        [Fact(DisplayName = nameof(PluginAssemblyName_MustBeSpecified))]
        public void PluginAssemblyName_MustBeSpecified()
        {
            ContainerSetup
               .Arrange((TestTypeResolver config) =>
               {
                   var appHostPlugin = new MockAppHostPlugin { AssemblyName = "" };
                   config.AddPlugin(appHostPlugin);
               })
               .Test(
                    c => c.Build(), (c, e) =>
                    {
                        e.Should().NotBeNull();
                        e.Should().BeOfType<ContainerException>();
                    });
        }

        [Fact(DisplayName = nameof(Container_CanOnlyBeStartedOnce))]
        public void Container_CanOnlyBeStartedOnce()
        {
            ContainerSetup
               .Arrange((TestTypeResolver config) =>
               {
                   var appHostPlugin = new MockAppHostPlugin { };
                   config.AddPlugin(appHostPlugin);
               })
               .Test(c =>
               {
                   var builtContainer = c.Build();
                   builtContainer.Start();
                   builtContainer.Start();
               }, 
               (c, e) =>
               {
                   e.Should().NotBeNull();
                   e.Should().BeOfType<ContainerException>();
               });
        }

        [Fact(DisplayName = nameof(NonStartedContainer_CannotBeStopped))]
        public void NonStartedContainer_CannotBeStopped()
        {
            ContainerSetup
               .Arrange((TestTypeResolver config) =>
               {
                   var appHostPlugin = new MockAppHostPlugin { };
                   config.AddPlugin(appHostPlugin);
               })
               .Test(c =>
               {
                   c.Build();
                   c.Stop();
               }, 
               (c, e) =>
               {
                   e.Should().NotBeNull();
                   e.Should().BeOfType<ContainerException>();
               });
        }

        [Fact(DisplayName = nameof(AllPluginManifests_MustHaveIdentityValue))]
        public void AllPluginManifests_MustHaveIdentityValue()
        {
            ContainerSetup
               .Arrange((TestTypeResolver config) =>
               {
                   var appHostPlugin = new MockAppHostPlugin { PluginId = null };
                   config.AddPlugin(appHostPlugin);
               })
               .Test(c => c.Build(),
               (c, e) =>
               {
                   e.Should().NotBeNull();
                   e.Should().BeOfType<ContainerException>();
               });
        }

        [Fact(DisplayName = nameof(PluginContainingType_CanBeQueried))]
        public void PluginContainingType_CanBeQueried()
        {
            ContainerSetup
               .Arrange((TestTypeResolver config) =>
               {
                   var appHostPlugin = new MockAppHostPlugin { PluginId = "__101" };
                   appHostPlugin.AddPluginType<MockTypeOneBasedOnKnownType>();
                   config.AddPlugin(appHostPlugin);
               })
               .Test(
                    c => c.Build(),
                    c =>
                    {
                        var plugIn = c.GetPluginForType(typeof(MockTypeOneBasedOnKnownType));
                        plugIn.Manifest.PluginId.Should().Be("__101");
                    }
               );
        }
    }
}
