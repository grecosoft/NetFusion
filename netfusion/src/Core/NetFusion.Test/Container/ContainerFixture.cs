using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Container;
using NetFusion.Test.Plugins;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetFusion.Base.Scripting;
using NetFusion.Base.Serialization;
using NetFusion.Base.Validation;
using NetFusion.Serialization;

// ReSharper disable MethodHasAsyncOverload
namespace NetFusion.Test.Container
{

    /// <summary>
    /// Test fixture for arranging, acting, and asserting a test instance of the
    /// application container.
    /// </summary>
    public class ContainerFixture 
    {
        // The services arranged for the unit-test that may override any services added by plugins.
        // This allows the unit test to override called services with mocked alternatives.
        internal IServiceCollection ServiceOverrides { get; }
        
        // The collection of services populated by the composite application. 
        internal IServiceCollection ComposedServices { get; private set; }
        
        internal IConfigurationBuilder ConfigBuilder { get; private set; }
        internal ITypeResolver Resolver { get; private set; }
        
        // Reference to the composite-container in which plugins are registered
        // and responsible for composing the plugins into a ICompositeApp that
        // can be used for the lifetime of the host.
        private CompositeContainer _container;
        private ICompositeApp _compositeApp;

        private ContainerFixture()
        {
            ServiceOverrides = new ServiceCollection();
        }
        
        
        //-- Unit Test Execution Methods:
        
        /// <summary>
        /// Executes a test delegate passed an instance of ContainerFixture that
        /// can be Arranged and Asserted.
        /// </summary>
        /// <param name="test">Method specified by the unit-test to execute logic against
        /// a created test-fixture instance.</param>
        public static void Test(Action<ContainerFixture> test)
        {
            if (test == null) throw new ArgumentNullException(nameof(test),
                "Test delegate cannot be null.");

            var fixture = CreateFixture(new TestTypeResolver());
            test(fixture);

            fixture._compositeApp?.Stop();
        }
        
        /// <summary>
        /// Creates a new test fixture for testing an application container.  The created
        /// instance is passed to the provided test method used to execute the unit-test.
        /// Once the test method completes, the test container is stopped.
        /// </summary>
        /// <param name="test">Method specified by the unit-test to execute logic against
        /// a created test-fixture instance.</param>
        public static async Task TestAsync(Func<ContainerFixture, Task> test)
        {
            if (test == null) throw new ArgumentNullException(nameof(test),
                "Test delegate cannot be null.");

            var fixture = CreateFixture(new TestTypeResolver());
            await test(fixture).ConfigureAwait(false);

            fixture._compositeApp?.Stop();
        }

        private static ContainerFixture CreateFixture(ITypeResolver resolver)
        {
            return new ContainerFixture
            {
                Resolver = resolver,
                ConfigBuilder = new ConfigurationBuilder(),
                ComposedServices = new ServiceCollection()
            };
        }
        
        //-- Properties and methods used ty the Arrange, Act, and Assert classes.

        internal bool IsCompositeContainerBuild => _container != null;

        /// <summary>
        /// Builds a Composite-Container using the Service-Collection associated
        /// with the test fixture.  
        /// </summary>
        internal CompositeContainer GetOrBuildContainer()
        {
            if (_container != null)
            {
                return _container;
            }
                
            var configuration = ConfigBuilder.Build();

            // The GenericHost used by Web and Console projects automatically
            // adds IConfiguration to the container.
            ComposedServices.AddSingleton(configuration);
                
            // Common services used by the majority of composite-applications or
            // default null-services for optional common services.
            ComposedServices.AddSingleton<IEntityScriptingService, NullEntityScriptingService>();
            ComposedServices.AddSingleton<IValidationService, ValidationService>();
            ComposedServices.AddSingleton<ISerializationManager, SerializationManager>();
            ComposedServices.AddLogging().AddOptions();
            
            _container = new CompositeContainer(ComposedServices, configuration);
            return _container;
        }
        
        /// <summary>
        /// Executes the compose method on the composite-container resulting in all
        /// plugins services being added to the corresponding service-collection.
        /// This also adds ICompositeApp to the service-collection representing the
        /// composite-application.  
        /// </summary>
        public void AssureContainerComposed()
        {
            if (!GetOrBuildContainer().IsComposed)
            {
                GetOrBuildContainer().Compose(Resolver);

                // Override any services provided by unit-tests.
                ComposedServices.Add(ServiceOverrides);
            }
        }

        /// <summary>
        /// Creates a service-provider from the populated service-collection and
        /// returns an instance of ICompositeApp.
        /// </summary>
        public ICompositeApp AppUnderTest
        {
            get
            {
                if (_compositeApp != null)
                {
                    return _compositeApp;
                }

                AssureContainerComposed();
                
                // Obtain reference to ICompositeApp representing composed application.
                var serviceProvider = ComposedServices.BuildServiceProvider();
                _compositeApp = serviceProvider.GetService<ICompositeApp>();
                return _compositeApp;
            }
        }

        /// <summary>
        /// Starts the built composite-application resulting in the starting of all plugin modules.
        /// </summary>
        public void AssureCompositeAppStarted()
        {
            if (!AppUnderTest.IsStarted)
            {
                AppUnderTest.Start();
            }
        }
        
        /// <summary>
        /// Allows the unit-test to arrange the test fixture under test.  
        /// </summary>
        public ContainerArrange Arrange => new ContainerArrange(this);
    }
}
