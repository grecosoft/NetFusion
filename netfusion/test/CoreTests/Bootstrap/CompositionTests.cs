using CoreTests.Bootstrap.Mocks;
using FluentAssertions;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using System.Linq;
using Xunit;

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
                fixture.Arrange.Container(c =>
                    {
                        // Plug-in type based on the type in the core plug-in.
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddPluginType<MockTypeOneBasedOnKnownType>();

                        // Core plug-in containing a module that is composed from type
                        // instances declared within another plug-in. 
                        var corePlugin = new MockCorePlugin();
                        corePlugin.AddModule<MockComposedModule>();
                        
                        c.RegisterPlugins(hostPlugin, corePlugin);
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
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddPluginType<MockTypeOneBasedOnKnownType>();
                        
                       var appPlugin = new MockAppPlugin();
                       appPlugin.AddPluginType<MockTypeTwoBasedOnKnownType>();
                       
                       var corePlugin = new MockCorePlugin();
                       corePlugin.AddPluginType<MockTypeThreeBasedOnKnownType>();
                       
                       c.RegisterPlugins(hostPlugin, appPlugin, corePlugin);

                       // This core plug-in contains a module that is composed
                       // from types contained within the above plug-ins.
                       var composedPlugin = new MockCorePlugin();
                       composedPlugin.AddModule<MockComposedModule>();
                        
                        c.RegisterPlugins(composedPlugin);
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
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddPluginType<MockTypeOneBasedOnKnownType>();
                        
                        var corePlugin = new MockCorePlugin();
                        corePlugin.AddPluginType<MockTypeThreeBasedOnKnownType>();

                        var corePlugin2 = new MockCorePlugin();
                        corePlugin2.AddPluginType<MockTypeTwoBasedOnKnownType>();

                        var appPlugin = new MockAppPlugin();
                        appPlugin.AddModule<MockComposedModule>();
                      
                        c.RegisterPlugins(hostPlugin, corePlugin, corePlugin2, appPlugin);
              
                    })
                    .Assert.PluginModule<MockComposedModule>(m =>
                    {
                        m.ImportedTypes.Should().NotBeNull();
                        m.ImportedTypes.Should().HaveCount(1);
                        m.ImportedTypes.OfType<MockTypeOneBasedOnKnownType>().Should().HaveCount(1);
                    });
            });
        }
    }
}

