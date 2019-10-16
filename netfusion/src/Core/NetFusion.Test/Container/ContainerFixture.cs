using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Container;
using NetFusion.Test.Plugins;
using System;
using System.Threading.Tasks;
using NetFusion.Base.Scripting;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Validation;
using NetFusion.Serialization;

namespace NetFusion.Test.Container
{

    /// <summary>
    /// Test fixture for arranging, acting, and asserting a test instance of the
    /// application container.
    /// </summary>
    public class ContainerFixture 
    {
        // Microsoft Abstractions: The service-collection populated with plugin
        // modules and configurations specific to a given unit-test.
        internal IServiceCollection Services { get; private set; }
        internal IConfigurationBuilder ConfigBuilder { get; private set; }

        internal ITypeResolver Resolver { get; private set; }
        
        // Reference to the composite-container in which plugins are registered
        // and is responsible for composing the plugins into a ICompositeApp 
        // that can be used for the lifetime of the host.
        private CompositeContainer _container;
        private ICompositeApp _compositeApp;
       
        private ContainerFixture() { }

        private static ContainerFixture CreateFixture(ITypeResolver resolver)
        {
            return new ContainerFixture
            {
                Resolver = resolver,
                ConfigBuilder = new ConfigurationBuilder(),

                Services = new ServiceCollection()
                    .AddLogging()
                    .AddOptions()
            };
        }

        /// <summary>
        /// Builds a Composite-Container using the Service-Collection associated
        /// with the test fixture.  This mimics the that happens when an application
        /// is built using the Generic Host builder and the ICompositeBuilder.
        /// </summary>
        public CompositeContainer ContainerUnderTest
        {
            get
            {
                if (_container != null)
                {
                    return _container;
                }
                
                var configuration = ConfigBuilder.Build();

                Services.AddSingleton(configuration);
                
                // Common services used by the majority of composite-applications or
                // default null-services for optional common services.
                Services.AddSingleton<IEntityScriptingService, NullEntityScriptingService>();
                Services.AddSingleton<IValidationService, ValidationService>();
                Services.AddSingleton<ISerializationManager, SerializationManager>();

                _container = new CompositeContainer(Services, configuration, new BootstrapLogger());
                return _container;
            }
        }

        /// <summary>
        /// Executes the compose method on the composite-container resulting in all
        /// plugins services being added to the corresponding service-collection.
        /// This also adds ICompositeApp to the service-collection representing the
        /// composite-application.  
        /// </summary>
        public void AssureContainerComposed()
        {
            if (!ContainerUnderTest.IsComposed)
            {
                ContainerUnderTest.Compose(Resolver);
            }
        }

        /// <summary>
        /// Creates a service-provider from the populated service-collection and
        /// instantiates and returns an instance of ICompositeApp.
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
                
                var serviceProvider = Services.BuildServiceProvider();
                _compositeApp = serviceProvider.GetService<ICompositeApp>();
                return _compositeApp;
            }
        }

        /// <summary>
        /// Starts the built composite-application resulting in the starting
        /// of all plugin modules.
        /// </summary>
        public void AssureCompositeAppStarted()
        {
            if (!AppUnderTest.IsStarted)
            {
                AssureContainerComposed();
                AppUnderTest.Start();
            }
        }
        
        /// <summary>
        /// Allows the unit-test to arrange the test fixture under test.  
        /// </summary>
        public ContainerArrange Arrange => new ContainerArrange(this);
        
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
        
        /// <summary>
        /// Executes a test delegate passed an instance of ContainerFixture that
        /// can be Arranged and Asserted.
        /// </summary>
        /// <param name="test">Method specified by the unit-test to execute logic against
        /// a created test-fixture instance.</param>
        public static void Execute(Action<ContainerFixture> test)
        {
            if (test == null) throw new ArgumentNullException(nameof(test),
                "Test delegate cannot be null.");

            var fixture = CreateFixture(new TypeResolver(new BootstrapLogger()));
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
        public static async Task ExecuteAsync(Func<ContainerFixture, Task> test)
        {
            if (test == null) throw new ArgumentNullException(nameof(test),
                "Test delegate cannot be null.");

            var fixture = CreateFixture(new TypeResolver(new BootstrapLogger()));
            await test(fixture).ConfigureAwait(false);

            fixture._compositeApp?.Stop();
        }      
    }
}
