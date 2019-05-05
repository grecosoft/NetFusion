using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using NetFusion.Test.Plugins;
using System;
using System.Threading.Tasks;

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
        private bool _isInit;

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

        public CompositeContainer ContainerUnderTest
        {
            get
            {
                var configuration = ConfigBuilder.Build();
                Services.AddSingleton(configuration);

                return _container ?? (_container = new CompositeContainer(
                    Services,
                    configuration,
                    new LoggerFactory(),
                    setGlobalReference: false));
            }
        }

        public void InitContainer()
        {
            if (!_isInit)
            {
                _isInit = true;
                ContainerUnderTest.Compose(Resolver).Start();
            }
        }

        /// <summary>
        /// Creates a new test fixture for testing an application container.  The created
        /// instance is passed to the provided test method used to execute the unit-test.
        /// Once the test method completes, the test container is disposed.
        /// </summary>
        /// <param name="test">Method specified by the unit-test to execute logic against
        /// a created test-fixture instance.</param>
        public static void Test(Action<ContainerFixture> test, Action<IConfigurationBuilder> config = null)
        {
            if (test == null) throw new ArgumentNullException(nameof(test),
                "Test delegate cannot be null.");

            var fixture = CreateFixture();
            config?.Invoke(fixture.ConfigBuilder);
            test(fixture);

            fixture._container?.Dispose();
        }

        /// <summary>
        /// Creates a new test fixture for testing an application container.  The created
        /// instance is passed to the provided test method used to execute the unit-test.
        /// Once the test method completes, the test container is disposed.
        /// </summary>
        /// <param name="test">Method specified by the unit-test to execute logic against
        /// a created test-fixture instance.</param>
        public static async Task TestAsync(Func<ContainerFixture, Task> test)
        {
            if (test == null) throw new ArgumentNullException(nameof(test),
                "Test delegate cannot be null.");

            var fixture = CreateFixture();
            await test(fixture).ConfigureAwait(false);

            fixture._container?.Dispose();
        }

        /// <summary>
        /// Allows the unit-test to arrange the test fixture under test.  
        /// </summary>
        public ContainerArrange Arrange => new ContainerArrange(this);

    }
}
