using CoreTests.Bootstrap.Mocks;
using FluentAssertions;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using System.Linq;
using Xunit;

namespace CoreTests.Bootstrap
{
    /// <summary>
    /// When developing plugins, especially core plugins, interfaces and abstract base types are
    /// defined for implementation by other plugins.  This allows the core plugin to execute code
    /// specific to another plugin. NetFusion defines the IPluginKnownType marker interface used
    /// to indicate these abstract types implemented by other plugins.  When NetFusion bootstraps,
    /// any module with IEnumerable properties deriving from IPluginKnownType will automatically 
    /// be populated with concrete object instances.
    ///
    /// NOTE:  This can be thought of as a very simplified version of Microsoft's MEF that solves
    /// the problem at hand.
    /// </summary>
    public class ModuleCompositionTests
    {
        /// <summary>
        /// A plugin module's known-type properties are automatically populated.
        /// </summary>
        [Fact]
        public void ModulesHavingKnownTypeProperties_WillBePopulated()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        // Plug-in type based on the type in the core plug-in.
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddPluginType<MockTypeOneBasedOnKnownType>();

                        // Core plug-in containing a module that is composed
                        // from type instances declared within other plugins. 
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
        /// considered a lower-level reusable infrastructure plugin, it can import types from
        /// other core, application component, and the application host plug-ins.  In this case,
        /// the core plug-in defines base types or interfaces that can be implemented by types
        /// contained in these other plug-ins.  
        /// </summary>
        [Fact]
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
                       // from types contained within the above plugins.
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
        /// Since application component services provide application specific services,
        /// their modules can only be composed from types belonging to other application
        /// plugins and the host.
        /// </summary>
        [Fact]
        public void AppPluginsComposed_FromOtherAppAndHostPluginTypesOnly()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        // The concrete types of the following two plugins should be found.
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddPluginType<MockTypeOneBasedOnKnownType>();

                        var appPlugin = new MockAppPlugin();
                        appPlugin.AddPluginType<MockTypeTwoBasedOnKnownType>();
                        
                        // Since the next two are core plugins, their types should not be
                        // found by the module contained within an application plugin.
                        var corePlugin = new MockCorePlugin();
                        corePlugin.AddPluginType<MockTypeThreeBasedOnKnownType>();

                        var corePlugin2 = new MockCorePlugin();
                        corePlugin2.AddPluginType<MockTypeFourBasedOnKnownType>();

                        // This is the application plugin with the module to be composed.
                        var composedPlugin = new MockAppPlugin();
                        appPlugin.AddModule<MockComposedModule>();
                      
                        c.RegisterPlugins(hostPlugin, appPlugin, corePlugin, corePlugin2, composedPlugin);
              
                    })
                    .Assert.PluginModule<MockComposedModule>(m =>
                    {
                        m.ImportedTypes.Should().NotBeNull();
                        m.ImportedTypes.Should().HaveCount(2);
                        m.ImportedTypes.OfType<MockTypeOneBasedOnKnownType>().Should().HaveCount(1);
                        m.ImportedTypes.OfType<MockTypeTwoBasedOnKnownType>().Should().HaveCount(1);
                    });
            });
        }
        
        /// <summary>
        /// Modules contained within the host plugin can only be composed from concrete types
        /// contained within itself.
        /// </summary>
        [Fact]
        public void HostPluginsComposed_FromOnlyItself()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        // The concrete types within only the host plugin should be found.
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddModule<MockComposedModule>();
                        hostPlugin.AddPluginType<MockTypeOneBasedOnKnownType>();
                        hostPlugin.AddPluginType<MockTypeTwoBasedOnKnownType>();

                        // The concrete types within the following plugins should not be found.
                        var appPlugin = new MockAppPlugin();
                        appPlugin.AddPluginType<MockTypeThreeBasedOnKnownType>();
                        
                        var corePlugin = new MockCorePlugin();
                        corePlugin.AddPluginType<MockTypeFourBasedOnKnownType>();
                        
                        c.RegisterPlugins(hostPlugin, appPlugin, corePlugin);
              
                    })
                    .Assert.PluginModule<MockComposedModule>(m =>
                    {
                        m.ImportedTypes.Should().NotBeNull();
                        m.ImportedTypes.Should().HaveCount(2);
                        m.ImportedTypes.OfType<MockTypeOneBasedOnKnownType>().Should().HaveCount(1);
                        m.ImportedTypes.OfType<MockTypeTwoBasedOnKnownType>().Should().HaveCount(1);
                    });
            });
        }
    }
}

