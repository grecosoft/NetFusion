using CoreTests.Bootstrap.Mocks;
using FluentAssertions;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using System;
using System.Linq;
using Xunit;

namespace CoreTests.Bootstrap
{
    /// <summary>
    /// Container unit-tests that validate how plug-ins are loaded and composed
    /// from types found within one another.  Plug-ins are identified as assemblies
    /// containing a type deriving from the IPluginManafist interface.  Each plug-in
    /// assembly can have one or more IPluginModule implementations.  A plug-in 
    /// module is composed from types found in other plug-ins by declared enumerable
    /// properties having an IPluginKnownType derived item type.
    /// </summary>
    public class CompositionTests
    {
        /// <summary>
        /// The composite application must have one and only one application host plug-in.
        /// </summary>
        [Fact(DisplayName = "Composite Application cannot have multiples Application Host Plug-Ins")]
        public void CompositeApplication_CannotHaveMultiple_AppHostPlugins()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>();
                    r.AddPlugin<MockAppHostPlugin>();
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
        /// The composite application must have one application host plug-in.
        /// </summary>
        [Fact(DisplayName = "Composite Application must have one Application host Plug-In")]
        public void CompositeApplication_MustHaveOne_AppHostPlugin()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                   
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
        /// The composite application will be constructed from one plug-in that hosts
        /// the application.
        /// </summary>
        [Fact(DisplayName = "Composite Application has Application Host Plug-In")]
        public void Composite_Application_Has_AppHostPlugin()
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
                .Assert.CompositeApp(ca =>
                {
                    ca.AppHostPlugin.Should().NotBeNull();
                });

            });        
        }

        /// <summary>
        /// The composite application can be constructed from several application
        /// specific plug-in components.  These plug-ins contain the main modules of the 
        /// application.
        /// </summary>
        [Fact(DisplayName = "Composite Application can have Multiple Application Component Plug-Ins")]
        public void CompositeApplication_CanHaveMultiple_AppComponentPlugins()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>();
                    r.AddPlugin<MockAppComponentPlugin>();
                    r.AddPlugin<MockAppComponentPlugin>();
                })
                .Act.OnContainer(c =>
                {
                    c.Build();
                })
                .Assert.CompositeApp(ca =>
                {
                    ca.AppComponentPlugins.Should().HaveCount(2);
                });

            });
        }

        /// <summary>
        /// The composite application can be constructed from several core plug-ins.
        /// These plug-ins provide reusable services for technical implementations
        /// that can optionally used by other plug-ins.
        /// </summary>
        [Fact(DisplayName = "Composite Application can have Multiple Core Plug-Ins")]
        public void CompositeApplication_CanHaveMuliple_CorePlugins()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>();
                    r.AddPlugin<MockCorePlugin>();
                    r.AddPlugin<MockCorePlugin>();
                })
                .Act.OnContainer(c =>
                {
                    c.Build();
                })
                .Assert.CompositeApp(ca =>
                {
                    ca.CorePlugins.Should().HaveCount(2);
                });
            });
        }

        /// <summary>
        /// Verifies that the AppContainer loads the application host plug-in types.
        /// </summary>
        [Fact (DisplayName = "Types Loaded for Application Plug-In")]
        public void TypesLoaded_ForAppPlugin()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockOneType>();
                })
                .Act.OnContainer(c =>
                {
                    c.Build();
                })
                .Assert.CompositeApp(ca =>
                {
                    // Type assignment:
                    ca.AppHostPlugin.PluginTypes.Should().HaveCount(1);
                    ca.AppHostPlugin.PluginTypes.Select(pt => pt.Type).Contains(typeof(MockOneType));

                    // Categorized Types:
                    ca.GetPluginTypes().Should().HaveCount(1);
                    ca.GetPluginTypes(PluginTypes.AppHostPlugin).Should().HaveCount(1);
                    ca.GetPluginTypes(PluginTypes.CorePlugin, PluginTypes.AppComponentPlugin).Should().HaveCount(0);
                });
            });
        }

        /// <summary>
        /// Verifies that the AppContainer loads the application component plug-in types.
        /// </summary>
        [Fact (DisplayName = "Types Loaded for Application Component Plugin")]
        public void TypesLoaded_ForAppComponentPlugin()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>();

                    r.AddPlugin<MockAppComponentPlugin>()
                        .AddPluginType<MockOneType>();
                })
                .Act.OnContainer(c =>
                {
                    c.Build();
                })
                .Assert.CompositeApp(ca =>
                {
                    var appComponentPlugin = ca.AppComponentPlugins.First();

                    // Type assignment:
                    appComponentPlugin.PluginTypes.Should().HaveCount(1);
                    appComponentPlugin.PluginTypes.Select(pt => pt.Type).Contains(typeof(MockOneType));

                    // Categorized Types:
                    ca.GetPluginTypes().Should().HaveCount(1);
                    ca.GetPluginTypes(PluginTypes.AppComponentPlugin).Should().HaveCount(1);
                    ca.GetPluginTypes(PluginTypes.CorePlugin, PluginTypes.AppHostPlugin).Should().HaveCount(0);
                });
            });
        }
       
        /// <summary>
        /// Verifies that the AppContainer loads the core plug-in plug-in types.
        /// </summary>
        [Fact (DisplayName = "Types loaded for Core Plug-In")]
        public void TypesLoaded_ForCorePlugin()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>();

                    r.AddPlugin<MockCorePlugin>()
                         .AddPluginType<MockOneType>()
                         .AddPluginType<MockTwoType>();
                })
                .Act.OnContainer(c =>
                {
                    c.Build();
                })
                .Assert.CompositeApp(ca =>
                {
                    var corePlugin = ca.CorePlugins.First();
                    corePlugin.PluginTypes.Should().HaveCount(2);

                    // Categorized Types:
                    ca.GetPluginTypes().Should().HaveCount(2);
                    ca.GetPluginTypes(PluginTypes.CorePlugin).Should().HaveCount(2);
                    ca.GetPluginTypes(PluginTypes.AppComponentPlugin, PluginTypes.AppHostPlugin).Should().HaveCount(0);
                });
            });
        }

        /// <summary>
        /// Each plug-in can define modules that are invoked during the bootstrap process.
        /// Modules define the functionally defined by a plug-in.
        /// </summary>
        [Fact (DisplayName = "Plug-In Modules called during Bootstrap")]
        public void PluginModules_CalledDuring_Bootstrap()
        {
            Action<Plugin, Type> assertOneModule = (p, type) => p.Modules.Should()
                .HaveCount(1)
                .And.Subject.First().Should().BeOfType(type);

            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>()
                         .AddPluginType<MockPluginOneModule>();

                    r.AddPlugin<MockAppComponentPlugin>()
                         .AddPluginType<MockPluginTwoModule>();

                    r.AddPlugin<MockCorePlugin>()
                         .AddPluginType<MockPluginThreeModule>();
                })
                .Act.OnContainer(c =>
                {
                    c.Build();
                })
                .Assert.CompositeApp(ca =>
                {
                    assertOneModule(ca.AppHostPlugin, typeof(MockPluginOneModule));
                    assertOneModule(ca.AppComponentPlugins.First(), typeof(MockPluginTwoModule));
                    assertOneModule(ca.CorePlugins.First(), typeof(MockPluginThreeModule));
                });
            });
        }

        /// <summary>
        /// A plug-in can have multiple modules to separate the configuration for different
        /// provided services.
        /// </summary>
        [Fact (DisplayName = "Plug-In can have Multiple Modules")]
        public void PluginCanHave_MultipleModules()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>();

                    r.AddPlugin<MockCorePlugin>()
                         .AddPluginType<MockPluginTwoModule>()
                         .AddPluginType<MockPluginThreeModule>();
                })
                .Act.OnContainer(c =>
                {
                    c.Build();
                })
                .Assert.CompositeApp(ca =>
                {
                    var pluginModules = ca.CorePlugins.First().Modules;
                    pluginModules.Should().HaveCount(2);
                    pluginModules.OfType<MockPluginTwoModule>().Should().HaveCount(1);
                    pluginModules.OfType<MockPluginThreeModule>().Should().HaveCount(1);
                });
            });
        }

        /// <summary>
        /// When the plug-in types are loaded, each .NET type is associated with a PluginType
        /// containing additional container specific type information.  Each PluginType is 
        /// associated with the plug-in containing the type.
        /// </summary>
        [Fact (DisplayName = "Plug-In Types associated with Plug-In")]
        public void PluginTypes_AssociatedWithPlugin()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Resolver(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>()
                       .AddPluginType<MockOneType>();
                })
                .Act.OnContainer(c =>
                {
                    c.Build();
                })
                .Assert.CompositeApp(ca =>
                {
                    var appHostPlugin = ca.AppHostPlugin;
                    appHostPlugin.PluginTypes.First().Plugin.Should().BeSameAs(appHostPlugin);
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
        [Fact (DisplayName = "Core Plug-Ins Composted from All Other Plug-in Types")]
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
                .Act.OnContainer(c =>
                {
                    c.Build();
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
        [Fact (DisplayName = "Application Plug-Ins composed from Other Plugin-In types Only")]
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
                .Act.OnContainer(c =>
                {
                    c.Build();
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
