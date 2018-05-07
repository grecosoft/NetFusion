using CoreTests.Bootstrap.Mocks;
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
    /// The following tests the interaction between the AppContainer and the convention
    /// based types used to configure the container.
    /// </summary>
    public class ContainerTests
    {
        [Fact(DisplayName = "All Plug-In Manifests must have Identity value")]
        public void AllPluginManifests_MustHaveIdentityValue()
        {
            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    var appHostPlugin = new MockAppHostPlugin { PluginId = null };
                    r.AddPlugin(appHostPlugin);
                }))
                .Act.BuildAndStartContainer()
                .Assert.Exception<ContainerException>(ex =>
                {

                });
            }));
        }

        /// <summary>
        /// Plug-in IDs must be unique.  Plug-ins can use these key values to identity information
        /// related to a plug-in. 
        /// </summary>
        [Fact(DisplayName = "Plug-In Id Values must be Unique")]
        public void PluginIdsValues_MustBeUnique()
        {
            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    var appHostPlugin = new MockAppHostPlugin { PluginId = "1" };
                    var corePlugin = new MockCorePlugin { PluginId = "1" };

                    r.AddPlugin((MockPlugin)appHostPlugin, (MockPlugin)corePlugin);
                }))
                .Act.BuildAndStartContainer()
                .Assert.Exception<ContainerException>(ex =>
                {
                    ex.Message.Should().Contain("Plug-in identity values must be unique.");
                });
            }));
        }

        /// <summary>
        /// Each plug-in has a name that is used when logging the
        /// container structure.
        /// </summary>
        [Fact(DisplayName = "Plug-In Name must be Specified")]
        public void PluginName_MustBeSpecified()
        {
            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    var appHostPlugin = new MockAppHostPlugin { Name = "" };
                    r.AddPlugin(appHostPlugin);
                }))
                .Act.BuildAndStartContainer()
                .Assert.Exception<ContainerException>(ex =>
                {
                    ex.Message.Should().Contain("All manifest instances must have AssemblyName and Name values.");
                });
            }));
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
            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    var appHostPlugin = new MockAppHostPlugin { AssemblyName = "" };
                    r.AddPlugin(appHostPlugin);
                }))
                .Act.BuildAndStartContainer()
                .Assert.Exception<ContainerException>(ex =>
                {
                    ex.Message.Should().Contain("All manifest instances must have AssemblyName and Name values.");
                });
            }));
        }

        /// <summary>
        /// The composite application must have one and only one application host plug-in.
        /// </summary>
        [Fact(DisplayName = "Composite Application cannot have multiples Application Host Plug-Ins")]
        public void CompositeApplication_CannotHaveMultiple_AppHostPlugins()
        {
            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>();
                    r.AddPlugin<MockAppHostPlugin>();
                }))
                .Act.BuildAndStartContainer()
                .Assert.Exception<ContainerException>(ex =>
                {
                    ex.Message.Should().Contain("More than one Host Application Plug-In manifest was found.");
                });
            }));
        }

        /// <summary>
        /// The composite application must have one application host plug-in.
        /// </summary>
        [Fact(DisplayName = "Composite Application must have one Application host Plug-In")]
        public void CompositeApplication_MustHaveOne_AppHostPlugin()
        {
            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {

                }))
                .Act.BuildAndStartContainer()
                .Assert.Exception<ContainerException>(ex =>
                {
                    ex.Message.Should().Contain("A Host Application Plug-In manifest could not be found");
                });
            }));
        }

        /// <summary>
        /// The composite application will be constructed from one plug-in that hosts
        /// the application.
        /// </summary>
        [Fact(DisplayName = "Composite Application has Application Host Plug-In")]
        public void Composite_Application_Has_AppHostPlugin()
        {
            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>();
                }))
                .Assert.CompositeApp(ca =>
                {
                    ca.AppHostPlugin.Should().NotBeNull();
                });
            }));
        }

        /// <summary>
        /// The composite application can be constructed from several application
        /// specific plug-in components.  These plug-ins contain the main modules of the 
        /// application.
        /// </summary>
        [Fact(DisplayName = "Composite Application can have Multiple Application Component Plug-Ins")]
        public void CompositeApplication_CanHaveMultiple_AppComponentPlugins()
        {
            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>();
                    r.AddPlugin<MockAppComponentPlugin>();
                    r.AddPlugin<MockAppComponentPlugin>();
                }))
                .Assert.CompositeApp(ca =>
                {
                    ca.AppComponentPlugins.Should().HaveCount(2);
                });
            }));
        }

        /// <summary>
        /// The composite application can be constructed from several core plug-ins.
        /// These plug-ins provide reusable services for technical implementations
        /// that can optionally used by other plug-ins.
        /// </summary>
        [Fact(DisplayName = "Composite Application can have Multiple Core Plug-Ins")]
        public void CompositeApplication_CanHaveMuliple_CorePlugins()
        {
            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>();
                    r.AddPlugin<MockCorePlugin>();
                    r.AddPlugin<MockCorePlugin>();
                }))
                .Assert.CompositeApp(ca =>
                {
                    ca.CorePlugins.Should().HaveCount(2);
                });
            }));
        }

        /// <summary>
        /// A plug-in can have multiple modules to separate the configuration for different
        /// provided services.
        /// </summary>
        [Fact(DisplayName = "Plug-In can have Multiple Modules")]
        public void PluginCanHave_MultipleModules()
        {
            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>();

                    r.AddPlugin<MockCorePlugin>()
                         .AddPluginType<MockPluginTwoModule>()
                         .AddPluginType<MockPluginThreeModule>();
                }))
                .Assert.CompositeApp(ca =>
                {
                    var pluginModules = ca.CorePlugins.First().Modules;
                    pluginModules.Should().HaveCount(2);
                    pluginModules.OfType<MockPluginTwoModule>().Should().HaveCount(1);
                    pluginModules.OfType<MockPluginThreeModule>().Should().HaveCount(1);
                });
            }));
        }

        /// <summary>
        /// Each plug-in can define modules that are invoked during the bootstrap process.
        /// Modules define the functionally defined by a plug-in.
        /// </summary>
        [Fact(DisplayName = "Plug-In Modules called during Bootstrap")]
        public void PluginModules_CalledDuring_Bootstrap()
        {
            Action<Plugin, Type> assertOneModule = (p, type) => p.Modules.Should()
                .HaveCount(1)
                .And.Subject.First().Should().BeOfType(type);

            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>()
                         .AddPluginType<MockPluginOneModule>();

                    r.AddPlugin<MockAppComponentPlugin>()
                         .AddPluginType<MockPluginTwoModule>();

                    r.AddPlugin<MockCorePlugin>()
                         .AddPluginType<MockPluginThreeModule>();
                }))
                .Assert.CompositeApp(ca =>
                {
                    assertOneModule(ca.AppHostPlugin, typeof(MockPluginOneModule));
                    assertOneModule(ca.AppComponentPlugins.First(), typeof(MockPluginTwoModule));
                    assertOneModule(ca.CorePlugins.First(), typeof(MockPluginThreeModule));
                });
            }));
        }

        /// <summary>
        /// Verifies that the AppContainer loads the application host plug-in types.
        /// </summary>
        [Fact(DisplayName = "Types Loaded for Application Plug-In")]
        public void TypesLoaded_ForAppPlugin()
        {
            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockOneType>();
                }))

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
            }));
        }

        /// <summary>
        /// Verifies that the AppContainer loads the application component plug-in types.
        /// </summary>
        [Fact(DisplayName = "Types Loaded for Application Component Plugin")]
        public void TypesLoaded_ForAppComponentPlugin()
        {
            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>();

                    r.AddPlugin<MockAppComponentPlugin>()
                        .AddPluginType<MockOneType>();
                }))
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
            }));
        }

        /// <summary>
        /// Verifies that the AppContainer loads the core plug-in plug-in types.
        /// </summary>
        [Fact(DisplayName = "Types loaded for Core Plug-In")]
        public void TypesLoaded_ForCorePlugin()
        {
            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>();

                    r.AddPlugin<MockCorePlugin>()
                         .AddPluginType<MockOneType>()
                         .AddPluginType<MockTwoType>();
                }))
                .Assert.CompositeApp(ca =>
                {
                    var corePlugin = ca.CorePlugins.First();
                    corePlugin.PluginTypes.Should().HaveCount(2);

                    // Categorized Types:
                    ca.GetPluginTypes().Should().HaveCount(2);
                    ca.GetPluginTypes(PluginTypes.CorePlugin).Should().HaveCount(2);
                    ca.GetPluginTypes(PluginTypes.AppComponentPlugin, PluginTypes.AppHostPlugin).Should().HaveCount(0);
                });
            }));
        }

        /// <summary>
        /// When the plug-in types are loaded, each .NET type is associated with a PluginType
        /// containing additional container specific type information.  Each PluginType is 
        /// associated with the plug-in containing the type.
        /// </summary>
        [Fact(DisplayName = "Plug-In Types associated with Plug-In")]
        public void PluginTypes_AssociatedWithPlugin()
        {
            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>()
                       .AddPluginType<MockOneType>();
                }))
                .Assert.CompositeApp(ca =>
                {
                    var appHostPlugin = ca.AppHostPlugin;
                    appHostPlugin.PluginTypes.First().Plugin.Should().BeSameAs(appHostPlugin);
                });
            }));
        }

        [Fact(DisplayName = "Plug-In containing type can be Queried")]
        public void PluginContainingType_CanBeQueried()
        {
            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    var appHostPlugin = new MockAppHostPlugin { PluginId = "__101" };
                    appHostPlugin.AddPluginType<MockTypeOneBasedOnKnownType>();
                    r.AddPlugin(appHostPlugin);
                }))
                .Assert.Container(c =>
                {
                    var composite = (IComposite)c;
                    var plugIn = composite.GetPluginContainingType(typeof(MockTypeOneBasedOnKnownType));
                    plugIn.Manifest.PluginId.Should().Be("__101");
                });
            }));
        }


        [Fact(DisplayName = "Container can only be Started Once")]
        public void Container_CanOnlyBeStartedOnce()
        {
            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    var appHostPlugin = new MockAppHostPlugin { };
                    r.AddPlugin(appHostPlugin);
                }))
                .Act.OnNonInitContainer(c =>
                {
                    var builtContainer = c.Build();
                    builtContainer.Start();
                    builtContainer.Start();
                })
                .Assert.Exception<ContainerException>(ex =>
                {
                    ex.Message.Should().Contain("The application container has already been started.");
                });
            }));
        }

        /// <summary>
        /// After the application container is built and started, each plug-in
        /// modules is given an opportunity to start.  This is where each 
        /// module is allowed to initialize any runtime services.  For example,
        /// this is where a service bus plug-in would create the needed queues
        /// and subscribe to consumers.
        /// </summary>
        [Fact(DisplayName = "When Container Started each Plug-in Module is Started")]
        public void WhenAppContainerStarted_EachPluginModuleStarted()
        {
            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockPluginOneModule>();

                    r.AddPlugin<MockCorePlugin>()
                        .AddPluginType<MockPluginTwoModule>();
                }))
                .Assert.CompositeApp(ca =>
                {
                    ca.AllPluginModules.OfType<MockPluginModule>()
                         .All(m => m.IsStarted).Should().BeTrue();
                });
            }));
        }

        [Fact(DisplayName = "Non-Started Container cannot be Stopped")]
        public void NonStartedContainer_CannotBeStopped()
        {
            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    var appHostPlugin = new MockAppHostPlugin { };
                    r.AddPlugin(appHostPlugin);
                }))
                .Act.OnNonInitContainer(c =>
                {
                    c.Stop();
                })
                .Assert.Exception<ContainerException>(ex =>
                {
                    ex.Message.Should().Contain("The application container has not been started.");
                });
            }));
        }

        /// <summary>
        /// When the application container is disposed, each plug-in module
        /// will be disposed.
        /// </summary>
        [Fact(DisplayName = "When Container disposed plug-in Modules are disposed")]
        public void AppContainerDisposed_PluginModules_AreDisposed()
        {

            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockPluginOneModule>();
                }))
                .Act.OnContainer(c =>
                {
                    c.Dispose();
                })
                .Assert.CompositeApp(ca =>
                {
                    var pluginModule = ca.AppHostPlugin.Modules.First();
                    (pluginModule as MockPluginModule).IsDisposed.Should().BeTrue();
                });
            }));
        }

        /// <summary>
        /// Once the application container is disposed, the associated service
        /// provider can no longer be accessed.
        /// </summary>
        [Fact(DisplayName = "Disposed Container cannot have Service Provider accessed")]
        public void DisposedAppContainer_CannotHave_ServicesAccessed()
        {
            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                fixture.Arrange.Resolver((Action<TestTypeResolver>)(r =>
                {
                    r.AddPlugin<MockAppHostPlugin>();
                }))
                .Act.OnContainer(c =>
                {
                    c.Dispose();
                    var s = c.ServiceProvider;
                })
                .Assert.Exception<ContainerException>(ex =>
                {
                    ex.Message.Should().Be(
                        "The application container has been disposed and can no longer be accessed.");
                });
            }));
        }
        // Test same for Log
        // Test same for LoggerFactory
        // Test same for CreateServiceScope
        // Test same for CreateValidator
        // Test same for Execute within service scope
    }
}
