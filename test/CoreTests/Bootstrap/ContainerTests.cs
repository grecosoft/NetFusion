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
        [Fact(DisplayName = "When Container disposed plug-in Modules are disposed")]
        public void AppContainerDisposed_PluginModules_AreDisposed()
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
                    c.Dispose();
                })
                .Assert.CompositeApp(ca =>
                {
                    var pluginModule = ca.AppHostPlugin.Modules.First();
                    (pluginModule as MockPluginModule).IsDisposed.Should().BeTrue();
                });
            });
        }

        /// <summary>
        /// Once the application container is disposed, the its provided
        /// services can no longer be accessed.
        /// </summary>
        [Fact(DisplayName = "Disposed Container cannot have Services accessed")]
        public void DisposedAppContainer_CannotHave_ServicesAccessed()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange.Resolver(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>();
                })
                .Act.OnContainer(c =>
                {                   
                    c.Build();
                    c.Dispose();
                    var s = c.Services;
                })
                .Assert.Exception<ContainerException>(ex => {
                    ex.Message.Should().Be(
                        "The application container has been disposed and can no longer be accessed.");
                });
            });           
        }

        /// <summary>
        /// After the application container is built and started, each plug-in
        /// modules is given an opportunity to start.  This is where each 
        /// module is allowed to initialize any runtime services.  For example,
        /// this is where a service bus plug-in would create the needed queues
        /// and subscribe to consumers.
        /// </summary>
        [Fact(DisplayName ="When Container Started each Plug-in Module is Started")]
        public void WhenAppContainerStarted_EachPluginModuleStarted()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockPluginOneModule>();

                    r.AddPlugin<MockCorePlugin>()
                        .AddPluginType<MockPluginTwoModule>();
                })
                .Act.OnContainer(c =>
                {
                    c.Build().Start();
                })
                .Assert.CompositeApp(ca =>
                {
                    ca.AllPluginModules.OfType<MockPluginModule>()
                         .All(m => m.IsStarted).Should().BeTrue();
                });
            });
        }

        /// <summary>
        /// A plugin defines interfaces, deriving from the base IKnownPluginType interface, to represent types it defines
        /// but are implemented by other plug-ins.  For example, a plugin in can defined a derived IKnownPluginType type 
        /// named IProcessor that other plugin-ins can implement.  The plug-in defining the IProcessor interface can get
        /// instances of all other concrete plug-in types, implementing this interface, by declaring a property as follows 
        /// on one of its modules:  
        /// 
        ///     IEnumerable<IProcessor> Processors { get; private set; }
        ///     
        /// The boot strap process will populate this property with instances of all found derived types.
        /// </summary>
        [Fact(DisplayName = "Known Type instance Discovered by Plug-in defining Known Type")]
        public void KnownTypeInstance_DiscoverdByPlugin_DefiningKnownType()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    // Plug-in type based on the type in the core plug-in.
                    r.AddPlugin<MockAppHostPlugin>()
                         .AddPluginType<MockTypeOneBasedOnKnownType>();

                    // A type that the core plug-in knows about.
                    r.AddPlugin<MockCorePlugin>()
                         .AddPluginType<MockKnownType>()
                         .AddPluginType<MockComposedModule>();
                })
                .Act.OnContainer(c =>
                {
                    c.Build();
                })
                .Assert.CompositeApp(ca =>
                {
                    // Verifies that the known plug-in type under test was discovered.
                    var discoveredTypes = ca.AppHostPlugin.PluginTypes.Where(pt => pt.DiscoveredByPlugins.Any());
                    discoveredTypes.Should().HaveCount(1);
                    discoveredTypes.First().Type.Should().Be(typeof(MockTypeOneBasedOnKnownType));
                });
            });
        }

        /// <summary>
        /// Each plug-in known type that is discovered by a plug-in module has its DiscoveredByPlugins property 
        /// populated.  This indicates what plug-in modules discovered the type and had part in its initialization.
        /// </summary>
        [Fact(DisplayName = "Known Type discovered by correct Plug-in")]
        public void KnowType_DiscoveredBy_CorrectPlugin()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    // Plug-in type based on the type in the core plug-in.
                    r.AddPlugin<MockAppHostPlugin>()
                          .AddPluginType<MockTypeOneBasedOnKnownType>();

                    // Core plug-in containing a module that is composed from type
                    // instances declared within another plug-in. 
                    r.AddPlugin<MockCorePlugin>()
                          .AddPluginType<MockComposedModule>()
                          .AddPluginType<MockKnownType>();
                })
                .Act.OnContainer(c =>
                {
                    c.Build().Start();
                })
                .Assert.CompositeApp(ca =>
                {
                    // Verifies that the known plug-in type under test was discovered by the expected plug-in.
                    var knownTypes = ca.AppHostPlugin.PluginTypes.Where(pt => pt.IsKnownType);
                    knownTypes.First().DiscoveredByPlugins.Should().HaveCount(1);
                    knownTypes.First().DiscoveredByPlugins.First().Manifest.Should().BeOfType<MockCorePlugin>();
                });
            });
        }

        /// <summary>
        /// A module indicates that it wants to discover all concrete instances of a given 
        /// IKnownPluginType by declaring an enumerable property of that know-type.  The 
        /// property will be populated with instances that are derived for the specified
        /// known type.
        /// </summary>
        [Fact(DisplayName = "Modules having Known Type Properties will be Populated")]
        public void ModulesHavingKnownTypeProperties_WillBePopulated()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    // Plug-in type based on the type in the core plug-in.
                    r.AddPlugin<MockAppHostPlugin>()
                          .AddPluginType<MockTypeOneBasedOnKnownType>();

                    // Core plug-in containing a module that is composed from type
                    // instances declared within another plug-in. 
                    r.AddPlugin<MockCorePlugin>()
                          .AddPluginType<MockComposedModule>()
                          .AddPluginType<MockKnownType>();
                })
                .Act.OnContainer(c =>
                {
                    c.Build().Start();
                })
                .Assert.PluginModule<MockComposedModule>(m =>
                {
                    m.ImportedTypes.Should().NotBeNull();
                    m.ImportedTypes.Should().HaveCount(1);
                    m.ImportedTypes.First().Should().BeOfType<MockTypeOneBasedOnKnownType>();
                });
            });
        }

        [Fact(DisplayName = "All Plug-In Manifests must have Identity value")]
        public void AllPluginManifests_MustHaveIdentityValue()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    var appHostPlugin = new MockAppHostPlugin { PluginId = null };
                    r.AddPlugin(appHostPlugin);
                })
                .Act.OnContainer(c =>
                {
                    c.Build();
                })
                .Assert.Exception<ContainerException>(ex =>
                {

                });
            });
        }

        /// <summary>
        /// Plug-in IDs must be unique.  Plug-ins can use these key values to identity information
        /// related to a plug-in. 
        /// </summary>
        [Fact(DisplayName = "Plug-In Id Values must be Unique")]
        public void PluginIdsValues_MustBeUnique()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    var appHostPlugin = new MockAppHostPlugin { PluginId = "1" };
                    var corePlugin = new MockCorePlugin { PluginId = "1" };

                    r.AddPlugin(appHostPlugin, corePlugin);
                })
                .Act.OnContainer(c =>
                {
                    c.Build();
                })
                .Assert.Exception<ContainerException>(ex =>
                {
                    ex.Message.Should().Contain("Plug-in identity values must be unique.");
                });
            });
        }

        /// <summary>
        /// Each plug-in has a name that is used when logging the
        /// container structure.
        /// </summary>
        [Fact(DisplayName = "Plug-In Name must be Specified")]
        public void PluginName_MustBeSpecified()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    var appHostPlugin = new MockAppHostPlugin { Name = "" };
                    r.AddPlugin(appHostPlugin);
                })
                .Act.OnContainer(c =>
                {
                    c.Build();
                })
                .Assert.Exception<ContainerException>(ex =>
                {
                    ex.Message.Should().Contain("All manifest instances must have AssemblyName and Name values.");
                });
            });
        }

        /// <summary>
        /// While every .NET type has an assembly, the only code in the design that
        /// has a dependency on the actual .NET assembly is the classes that implement
        /// the ITypeResolver interface.  The type resolver specifies the name of the
        /// type's assembly which must be unique among plug-ins.
        /// </summary>
        [Fact(DisplayName = "Plug-In Assembly Name must be Specified")]
        public void PluginAssemblyName_MustBeSpecified()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    var appHostPlugin = new MockAppHostPlugin { AssemblyName = "" };
                    r.AddPlugin(appHostPlugin);
                })
                .Act.OnContainer(c =>
                {
                    c.Build();
                })
                .Assert.Exception<ContainerException>(ex =>
                {
                    ex.Message.Should().Contain("All manifest instances must have AssemblyName and Name values.");
                });
            });
        }

        [Fact(DisplayName = "Container can only be Started Once")]
        public void Container_CanOnlyBeStartedOnce()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    var appHostPlugin = new MockAppHostPlugin { };
                    r.AddPlugin(appHostPlugin);
                })
                .Act.OnContainer(c =>
                {
                    var builtContainer = c.Build();
                    builtContainer.Start();
                    builtContainer.Start();
                })
                .Assert.Exception<ContainerException>(ex =>
                {
                    ex.Message.Should().Contain("The application container plug-in modules have already been started.");
                });
            });
        }

        [Fact(DisplayName = "Non-Started Container cannot be Stopped")]
        public void NonStartedContainer_CannotBeStopped()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    var appHostPlugin = new MockAppHostPlugin { };
                    r.AddPlugin(appHostPlugin);
                })
                .Act.OnContainer(c =>
                {
                    c.Build();
                    c.Stop();
                })
                .Assert.Exception<ContainerException>(ex =>
                {
                    ex.Message.Should().Contain("The application container plug-in modules have not been started.");
                });
            });
        }

        [Fact(DisplayName = "Plug-In containing type can be Queried")]
        public void PluginContainingType_CanBeQueried()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    var appHostPlugin = new MockAppHostPlugin { PluginId = "__101" };
                    appHostPlugin.AddPluginType<MockTypeOneBasedOnKnownType>();
                    r.AddPlugin(appHostPlugin);
                })
                .Act.OnContainer(c =>
                {
                    c.Build();
                })
                .Assert.Container(c =>
                {
                    var composite = (IComposite)c;
                    var plugIn = composite.GetPluginContainingType(typeof(MockTypeOneBasedOnKnownType));
                    plugIn.Manifest.PluginId.Should().Be("__101");
                });
            });
        }
    }
}
