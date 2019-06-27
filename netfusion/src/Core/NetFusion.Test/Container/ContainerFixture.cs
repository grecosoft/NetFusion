using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Container;
using NetFusion.Test.Plugins;
using System;
using System.Threading.Tasks;
using NetFusion.Base.Scripting;
using NetFusion.Base.Serialization;
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
        internal TestTypeResolver Resolver { get; private set; }
        internal IServiceCollection Services { get; private set; }
        internal IConfigurationBuilder ConfigBuilder { get; private set; }

        private CompositeContainer _container;
        private ICompositeApp _compositeApp;
       
        private ContainerFixture() { }

        private static ContainerFixture CreateFixture()
        {
            return new ContainerFixture
            {
                Resolver = new TestTypeResolver(),
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
                
                Services.AddSingleton<IEntityScriptingService, NullEntityScriptingService>();
                Services.AddSingleton<IValidationService, ValidationService>();
                Services.AddSingleton<ISerializationManager, SerializationManager>();

                _container = new CompositeContainer(Services, configuration);
                return _container;
            }
        }

        public void AssureContainerComposed()
        {
            if (!ContainerUnderTest.IsComposted)
            {
                ContainerUnderTest.Compose(Resolver);
            }
        }

        /// <summary>
        /// Creates a Composite-Application using the configured 
        /// </summary>
        public ICompositeApp AppUnderTest
        {
            get
            {
                if (_compositeApp != null)
                {
                    return _compositeApp;
                }

                if (! ContainerUnderTest.IsComposted)
                {
                    ContainerUnderTest.Compose(Resolver);
                }
                
                var serviceProvider = Services.BuildServiceProvider();
                _compositeApp = serviceProvider.GetService<ICompositeApp>();
                _compositeApp.Start();
                return _compositeApp;
            }
        }

        /// <summary>
        /// Allows the unit-test to arrange the test fixture under test.  
        /// </summary>
        public ContainerArrange Arrange => new ContainerArrange(this);

        /// <summary>
        /// Creates a new test fixture for testing an application container.  The created
        /// instance is passed to the provided test method used to execute the unit-test.
        /// Once the test method completes, the test container is stopped.
        /// </summary>
        /// <param name="test">Method specified by the unit-test to execute logic against
        /// a created test-fixture instance.</param>
        /// <param name="config"></param>
        public static void Test(Action<ContainerFixture> test, Action<IConfigurationBuilder> config = null)
        {
            if (test == null) throw new ArgumentNullException(nameof(test),
                "Test delegate cannot be null.");

            var fixture = CreateFixture();
            config?.Invoke(fixture.ConfigBuilder);
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

            var fixture = CreateFixture();
            await test(fixture).ConfigureAwait(false);

            fixture._compositeApp?.Stop();
        }
    }
}
