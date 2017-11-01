﻿using CoreTests.Bootstrap.Mocks;
using FluentAssertions;
using NetFusion.Bootstrap.Container;
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
        [Fact(DisplayName = nameof(Can_Not_Have_Multiple_AppHostPlugins))]
        public void Can_Not_Have_Multiple_AppHostPlugins()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>();
                    config.AddPlugin<MockAppHostPlugin>();
                })
                .Test(
                    c => c.Build(), 
                    (c, e) =>
                    {
                        e.Should().BeOfType<ContainerException>();
                    });
        }

        /// <summary>
        /// The composite application must have one application host plug-in.
        /// </summary>
        [Fact(DisplayName = nameof(Must_Have_One_AppHostPlugin))]
        public void Must_Have_One_AppHostPlugin()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) => { })
                .Test(
                    c => c.Build(),
                    (c, e) =>
                    {
                        e.Should().BeOfType<ContainerException>();
                    });
        }

        /// <summary>
        /// The composite application will be constructed from one plug-in that hosts
        /// the application.
        /// </summary>
        [Fact(DisplayName = nameof(Composite_Application_Has_AppHost_Plugin))]
        public void Composite_Application_Has_AppHost_Plugin()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>();
                })
                .Test(
                    c => c.Build(),
                    (CompositeApplication ca) => ca.AppHostPlugin.Should().NotBeNull());
        }

        /// <summary>
        /// The composite application can be constructed from several application
        /// specific plug-in components.  These plug-ins contain the main modules of the 
        /// application.
        /// </summary>
        [Fact(DisplayName = nameof(Composite_Application_Can_Have_Several_App_Component_Plugins))]
        public void Composite_Application_Can_Have_Several_App_Component_Plugins()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>();
                    config.AddPlugin<MockAppComponentPlugin>();
                    config.AddPlugin<MockAppComponentPlugin>();
                })
                .Test(
                    c => c.Build(),
                    (CompositeApplication ca) => ca.AppComponentPlugins.Should().HaveCount(2));
        }

        /// <summary>
        /// The composite application can be constructed from several core plug-ins.
        /// These plug-ins provide reusable services for technical implementations
        /// that can optionally used by other plug-ins.
        /// </summary>
        [Fact(DisplayName = nameof(Composite_Application_Can_Have_Several_Core_Plugins))]
        public void Composite_Application_Can_Have_Several_Core_Plugins()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>();
                    config.AddPlugin<MockCorePlugin>();
                    config.AddPlugin<MockCorePlugin>();
                })
                .Test(
                    c => c.Build(),
                    (CompositeApplication ca) => ca.CorePlugins.Should().HaveCount(2));
        }

        /// <summary>
        /// Verifies that the AppContainer loads the application host plug-in types.
        /// </summary>
        [Fact]
        public void TypesLoaded_ForAppPlugin()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockOneType>();
                })
                .Test(
                    c => c.Build(),
                    (CompositeApplication ca) =>
                    {
                        // Type assignment:
                        ca.AppHostPlugin.PluginTypes.Should().HaveCount(1);
                        ca.AppHostPlugin.PluginTypes.Select(pt => pt.Type).Contains(typeof(MockOneType));

                        // Categorized Types:
                        ca.GetPluginTypes().Should().HaveCount(1);
                        ca.GetPluginTypes(PluginTypes.AppHostPlugin).Should().HaveCount(1);
                        ca.GetPluginTypes(PluginTypes.CorePlugin, PluginTypes.AppComponentPlugin).Should().HaveCount(0);
                    });
        }

        /// <summary>
        /// Verifies that the AppContainer loads the application component plug-in types.
        /// </summary>
        [Fact]
        public void TypesLoaded_ForAppComponentPlugin()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>();

                    config.AddPlugin<MockAppComponentPlugin>()
                        .AddPluginType<MockOneType>();
                })
                .Test(
                    c => c.Build(),
                    (CompositeApplication ca) =>
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
        }

        /// <summary>
        /// Verifies that the AppContainer loads the core plug-in plug-in types.
        /// </summary>
        [Fact]
        public void TypesLoaded_ForCorePlugin()
        {
            ContainerSetup
               .Arrange((TestTypeResolver config) =>
               {
                   config.AddPlugin<MockAppHostPlugin>();

                   config.AddPlugin<MockCorePlugin>()
                        .AddPluginType<MockOneType>()
                        .AddPluginType<MockTwoType>();
               })
               .Test(
                    c => c.Build(),
                    (CompositeApplication ca) =>
                    {
                        var corePlugin = ca.CorePlugins.First();
                        corePlugin.PluginTypes.Should().HaveCount(2);

                        // Categorized Types:
                        ca.GetPluginTypes().Should().HaveCount(2);
                        ca.GetPluginTypes(PluginTypes.CorePlugin).Should().HaveCount(2);
                        ca.GetPluginTypes(PluginTypes.AppComponentPlugin, PluginTypes.AppHostPlugin).Should().HaveCount(0);
                    });
        }

        /// <summary>
        /// Each plug-in can define modules that are invoked during the bootstrap process.
        /// Modules define the functionally defined by a plug-in.
        /// </summary>
        [Fact]
        public void ModulesUsedTo_BootstrapPlugin()
        {
            Action<Plugin, Type> assertOneModule = (p, type) => p.Modules.Should()
                .HaveCount(1)
                .And.Subject.First().Should().BeOfType(type);

            ContainerSetup
               .Arrange((TestTypeResolver config) =>
               {
                   config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockPluginOneModule>();

                   config.AddPlugin<MockAppComponentPlugin>()
                        .AddPluginType<MockPluginTwoModule>();

                   config.AddPlugin<MockCorePlugin>()
                        .AddPluginType<MockPluginThreeModule>();
               })
               .Test(
                    c => c.Build(),
                    (CompositeApplication ca) =>
                    {
                        assertOneModule(ca.AppHostPlugin, typeof(MockPluginOneModule));
                        assertOneModule(ca.AppComponentPlugins.First(), typeof(MockPluginTwoModule));
                        assertOneModule(ca.CorePlugins.First(), typeof(MockPluginThreeModule));
                    });
        }

        /// <summary>
        /// A plug-in can have multiple modules to separate the configuration for different
        /// provided services.
        /// </summary>
        [Fact]
        public void PluginCanHave_MultipleModules()
        {
            ContainerSetup
               .Arrange((TestTypeResolver config) =>
               {
                   config.AddPlugin<MockAppHostPlugin>();

                   config.AddPlugin<MockCorePlugin>()
                        .AddPluginType<MockPluginTwoModule>()
                        .AddPluginType<MockPluginThreeModule>();
               })
               .Test(
                    c => c.Build(),
                    (CompositeApplication ca) =>
                    {
                        var pluginModules = ca.CorePlugins.First().Modules;
                        pluginModules.Should().HaveCount(2);
                        pluginModules.OfType<MockPluginTwoModule>().Should().HaveCount(1);
                        pluginModules.OfType<MockPluginThreeModule>().Should().HaveCount(1);
                    });
        }

        /// <summary>
        /// When the plug-in types are loaded, each .NET type is associated with a PluginType
        /// containing additional container specific type information.  Each PluginType is 
        /// associated with the plug-in containing the type.
        /// </summary>
        [Fact]
        public void PluginTypes_AssociatedWithPlugin()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockOneType>();
                })
                .Test(
                    c => c.Build(),
                    (CompositeApplication ca) =>
                    {
                        var appHostPlugin = ca.AppHostPlugin;
                        appHostPlugin.PluginTypes.First().Plugin.Should().BeSameAs(appHostPlugin);
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
        [Fact]
        public void CorePluginsComposed_FromAllOtherPluginTypes()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockTypeOneBasedOnKnownType>();

                    config.AddPlugin<MockAppComponentPlugin>()
                        .AddPluginType<MockTypeTwoBasedOnKnownType>();

                    config.AddPlugin<MockCorePlugin>()
                        .AddPluginType<MockTypeThreeBasedOnKnownType>();

                    // This core plug-in contains a module that is composed
                    // from types contained within the above plug-ins.
                    config.AddPlugin<MockCorePlugin>()
                        .AddPluginType<MockComposedModule>();
                })
                .Test(
                    c => c.Build(),
                    (MockComposedModule m) =>
                    {
                        m.ImportedTypes.Should().NotBeNull();
                        m.ImportedTypes.Should().HaveCount(3);
                        m.ImportedTypes.OfType<MockTypeOneBasedOnKnownType>().Should().HaveCount(1);
                        m.ImportedTypes.OfType<MockTypeTwoBasedOnKnownType>().Should().HaveCount(1);
                        m.ImportedTypes.OfType<MockTypeThreeBasedOnKnownType>().Should().HaveCount(1);
                    });
        }

        /// <summary>
        /// Since application component services provide higher-level services, their modules can
        /// only be composed from types belonging to other application plug-ins.
        /// </summary>
        [Fact]
        public void AppPluginsComposed_FromOnlyOtherAppPluginTypes()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockTypeOneBasedOnKnownType>();

                    config.AddPlugin<MockCorePlugin>()
                        .AddPluginType<MockTypeThreeBasedOnKnownType>();

                    config.AddPlugin<MockCorePlugin>()
                        .AddPluginType<MockTypeTwoBasedOnKnownType>();

                    config.AddPlugin<MockAppComponentPlugin>()
                        .AddPluginType<MockComposedModule>();
                })
                .Test(
                    c => c.Build(),
                    (MockComposedModule m) =>
                    {
                        m.ImportedTypes.Should().NotBeNull();
                        m.ImportedTypes.Should().HaveCount(1);
                        m.ImportedTypes.OfType<MockTypeOneBasedOnKnownType>().Should().HaveCount(1);
                    });
        }
    }
}
