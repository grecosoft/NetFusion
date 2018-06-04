using CoreTests.Bootstrap.Mocks;
using FluentAssertions;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using System.Linq;
using Xunit;

#pragma warning disable 1570
namespace CoreTests.Bootstrap
{
    public class CompositionTests
    {
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
                    .Assert.PluginModule<MockComposedModule>(m =>
                    {
                        m.ImportedTypes.Should().NotBeNull();
                        m.ImportedTypes.Should().HaveCount(1);
                        m.ImportedTypes.First().Should().BeOfType<MockTypeOneBasedOnKnownType>();
                    });
            });
        }

        /// <summary>
        /// Modules can be composed of types found in other plug-ins.  A Core plug-in type can
        /// can be composed from types found in all types of plug-ins.  Since a core plug-in is
        /// considered a lower-level infrastructure plug-in (i.e MongoDb Plug-in), it can import
        /// types from other core, application component, and the application host plug-ins.  In
        /// this case, the core plug-in defines base types or interfaces that can be implemented
        /// by types contained in these other plug-ins.  
        /// </summary>
        [Fact(DisplayName = "Core Plug-Ins Composted from All Other Plug-in Types")]
        public void CorePluginsComposed_FromAllOtherPluginTypes()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                    {
                        r.AddPlugin<MockAppHostPlugin>()
                            .AddPluginType<MockTypeOneBasedOnKnownType>();

                        r.AddPlugin<MockAppComponentPlugin>()
                            .AddPluginType<MockTypeTwoBasedOnKnownType>();

                        r.AddPlugin<MockCorePlugin>()
                            .AddPluginType<MockTypeThreeBasedOnKnownType>();

                        // This core plug-in contains a module that is composed
                        // from types contained within the above plug-ins.
                        r.AddPlugin<MockCorePlugin>()
                            .AddPluginType<MockComposedModule>();
                    })
                    .Assert.PluginModule<MockComposedModule>(m =>
                    {
                        m.ImportedTypes.Should().NotBeNull();
                        m.ImportedTypes.Should().HaveCount(3);
                        m.ImportedTypes.OfType<MockTypeOneBasedOnKnownType>().Should().HaveCount(1);
                        m.ImportedTypes.OfType<MockTypeTwoBasedOnKnownType>().Should().HaveCount(1);
                        m.ImportedTypes.OfType<MockTypeThreeBasedOnKnownType>().Should().HaveCount(1);
                    });
            });
        }

        /// <summary>
        /// Since application component services provide higher-level services, their modules can
        /// only be composed from types belonging to other application plug-ins.
        /// </summary>
        [Fact(DisplayName = "Application Plug-Ins composed from Other Plugin-In types Only")]
        public void AppPluginsComposed_FromOnlyOtherAppPluginTypesOnly()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                    {
                        r.AddPlugin<MockAppHostPlugin>()
                            .AddPluginType<MockTypeOneBasedOnKnownType>();

                        r.AddPlugin<MockCorePlugin>()
                            .AddPluginType<MockTypeThreeBasedOnKnownType>();

                        r.AddPlugin<MockCorePlugin>()
                            .AddPluginType<MockTypeTwoBasedOnKnownType>();

                        r.AddPlugin<MockAppComponentPlugin>()
                            .AddPluginType<MockComposedModule>();
                    })
                    .Assert.PluginModule<MockComposedModule>(m =>
                    {
                        m.ImportedTypes.Should().NotBeNull();
                        m.ImportedTypes.Should().HaveCount(1);
                        m.ImportedTypes.OfType<MockTypeOneBasedOnKnownType>().Should().HaveCount(1);
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
                    .Assert.CompositeApp(ca =>
                    {
                        // Verifies that the known plug-in type under test was discovered.
                        var discoveredTypes = ca.AppHostPlugin.PluginTypes
                            .Where(pt => pt.DiscoveredByPlugins.Any())
                            .ToArray();
                        
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
                    .Assert.CompositeApp(ca =>
                    {
                        // Verifies that the known plug-in type under test was discovered by the expected plug-in.
                        var knownTypes = ca.AppHostPlugin.PluginTypes
                            .Where(pt => pt.IsKnownTypeImplementation)
                            .ToArray();
                        
                        knownTypes.First().DiscoveredByPlugins.Should().HaveCount(1);
                        knownTypes.First().DiscoveredByPlugins.First().Manifest.Should().BeOfType<MockCorePlugin>();
                    });
            });
        } 
    }
}
