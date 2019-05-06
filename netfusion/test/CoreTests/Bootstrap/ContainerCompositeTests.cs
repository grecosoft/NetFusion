using System.Linq;
using CoreTests.Bootstrap.Mocks;
using FluentAssertions;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

namespace CoreTests.Bootstrap
{
    /// <summary>
    /// The following tests the constraints that must exist when composing a
    /// CompositeContainer from a set of plugins.
    /// </summary>
    public class ContainerTests
    {
        /// <summary>
        /// Not often used by plugin-implementations, but can be used for cases where
        /// instances such as Queues, should be created on a Topic, and be scoped to
        /// a specific application.  In this case, the PluginId of the host plugin is
        /// most often used.
        /// </summary>
        [Fact(DisplayName = "All Plug-In Manifests must have Identity value")]
        public void AllPluginManifests_MustHaveIdentityValue()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(r =>
                    {
                        var testPlugin = new MockHostPlugin();
                        testPlugin.SetPluginId(null);
                        
                        r.RegisterPlugins(testPlugin);
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {

                    });
            });
        }

        [Fact(DisplayName = "Plug-In Id Values must be Unique")]
        public void PluginIdsValues_MustBeUnique()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();
                        var corePlugin = new MockCorePlugin();
                        
                        hostPlugin.SetPluginId("1");
                        corePlugin.SetPluginId("1");
                        
                        c.RegisterPlugins(hostPlugin, corePlugin);
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.Message.Should().Contain("Plug-in identity values must be unique.");
                    });
            });
        }

        /// <summary>
        /// Each plug-in has a name that is used when logging the composite container structure.
        /// </summary>
        [Fact(DisplayName = "Plug-In Name must be Specified")]
        public void PluginName_MustBeSpecified()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.SetPluginName(null);
                        
                        c.RegisterPlugins(hostPlugin);
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.Message.Should().Contain("All plugins must have AssemblyName and Name values.");
                    });
            });
        }

        /// <summary>
        /// While every .NET type has an assembly, the only code in the design that
        /// has a dependency on the actual .NET assembly is the class implementing
        /// the ITypeResolver interface.  The type resolver specifies the name of the
        /// type's assembly which must be unique among plug-ins.
        /// </summary>
        [Fact(DisplayName = "Plug-In Assembly Name must be Specified")]
        public void PluginAssemblyName_MustBeSpecified()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var testPlugin = new MockHostPlugin {AssemblyName = null};
                        c.RegisterPlugins(testPlugin);
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.Message.Should().Contain("All plugins must have AssemblyName and Name values.");
                    });
            });
        }

        /// <summary>
        /// The composite application must have one and only one application host plug-in.
        /// </summary>
        [Fact(DisplayName = "Composite Application cannot have multiples Application Host Plug-Ins")]
        public void CompositeApplication_CannotHaveMultiple_AppHostPlugins()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin1 = new MockHostPlugin();
                        var hostPlugin2 = new MockHostPlugin();
                        
                        c.RegisterPlugins(hostPlugin1, hostPlugin2);
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.Message.Should().Contain("There can only be one host plugin.");
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
                fixture.Arrange.Container(r =>
                    {

                    })
                    .Act.BuildAndStartContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.Message.Should().Contain("he composite application must have one host plugin type.");
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
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                    })
                    .Assert.CompositeApp(ca =>
                    {
                        ca.HostPlugin.Should().NotBeNull();
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
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                        c.RegisterPlugins(new MockApplicationPlugin());
                        c.RegisterPlugins(new MockApplicationPlugin());
                    })
                    .Assert.CompositeApp(ca =>
                    {
                        ca.AppPlugins.Should().HaveCount(2);
                    });
            });
        }

        /// <summary>
        /// The composite application can be constructed from several core plug-ins.
        /// These plug-ins provide reusable services for technical implementations
        /// that can optionally used by other plug-ins.
        /// </summary>
        [Fact(DisplayName = "Composite Application can have Multiple Core Plug-Ins")]
        public void CompositeApplication_CanHaveMultiple_CorePlugins()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                        c.RegisterPlugins(new MockCorePlugin());
                        c.RegisterPlugins(new MockCorePlugin());
                    })
                    .Assert.CompositeApp(ca =>
                    {
                        ca.CorePlugins.Should().HaveCount(2);
                    });
            });
        }

        /// <summary>
        /// A plug-in can have multiple modules to separate the configuration for different
        /// provided services.
        /// </summary>
        [Fact(DisplayName = "Plug-In can have Multiple Modules")]
        public void PluginCanHave_MultipleModules()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                        
                        var testPlugin = new MockCorePlugin();
                        testPlugin.AddModule<MockPluginTwoModule>();
                        testPlugin.AddModule<MockPluginThreeModule>();

                        c.RegisterPlugins(testPlugin);

                    })
                    .Assert.CompositeApp(ca =>
                    {
                        var pluginModules = ca.CorePlugins.First().Modules.ToArray();
                        pluginModules.Should().HaveCount(2);
                        pluginModules.OfType<MockPluginTwoModule>().Should().HaveCount(1);
                        pluginModules.OfType<MockPluginThreeModule>().Should().HaveCount(1);
                    });
            });
        }
    }
}
