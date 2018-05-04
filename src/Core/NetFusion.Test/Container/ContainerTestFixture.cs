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

        private AppContainer _container;
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

        public AppContainer ContainerUnderTest
        {
            get
            {
                var configuration = ConfigBuilder.Build();
                Services.AddSingleton(configuration);


                return _container ?? (_container = new AppContainer(
                    Services,
                    configuration,
                    new LoggerFactory(),
                    Resolver,
                    setGlobalReference: false));
            }
        }

        public void InitContainer()
        {
            if (!_isInit)
            {
                ContainerUnderTest.Build().Start();
            }
        }





            

        public static void Test2(Action<ContainerFixture> test)
        {
            if (test == null) throw new ArgumentNullException(nameof(test),
                "Test delegate cannot be null.");

            var fixture = CreateFixture();
            test(fixture);

            fixture._container.Dispose();
        }

        /// <summary>
        /// Creates a new test fixture for testing an application container.  The created
        /// instance is passed to the provided fixture method used to execute the unit-test.
        /// Once the fixture method completed, the test container is disposed.
        /// </summary>
        /// <param name="fixture">Method specified by the unit-test to execute logic against
        /// a created test-fixture instance.</param>
        public static async Task TestAsync(Func<ContainerFixture, Task> test)
        {
            if (test == null) throw new ArgumentNullException(nameof(test),
                "Test delegate cannot be null.");

            var fixture = CreateFixture();
            await test(fixture);

            fixture._container.Dispose();
        }



        /// <summary>
        /// Returns a new test fixture with a created application container that
        /// can be arranged for testing a specific scenario.
        /// </summary>
        private static ContainerFixture CreateTestFixture()
        {
            var resolver = new TestTypeResolver();
            var services = new ServiceCollection()
                .AddLogging()
                .AddOptions();

            return new ContainerFixture
            {
                Resolver = resolver,
                _container = new AppContainer(
                    services, 
                    new ConfigurationBuilder().Build(),
                    new LoggerFactory(), 
                    resolver, 
                    setGlobalReference: false)
            };
        }

        /// <summary>
        /// Creates a new test fixture for testing an application container.  The created
        /// instance is passed to the provided fixture method used to execute the unit-test.
        /// Once the fixture method completed, the test container is disposed.
        /// </summary>
        /// <param name="fixture">Method specified by the unit-test to execute logic against
        /// a created test-fixture instance.</param>
        public static void Test(Action<ContainerFixture> fixture)
        {
            var testFixture = CreateTestFixture();

            if (fixture == null) throw new ArgumentNullException(nameof(fixture), 
                "Test fixture cannot be null.");

            fixture(testFixture);

            testFixture._container.Dispose();           
        }

        /// <summary>
        /// Allows the unit-test to arrange the type-resolver and application container under test.  
        /// The type-resolver basically abstracts the logic for loading plug-in types without being 
        /// dependent on .NET assemblies.  This makes unit-testing much easier.
        /// </summary>
        public ContainerArrange Arrange => new ContainerArrange(Resolver, _container);
        public ContainerArrange Arrange2 => new ContainerArrange(this);

    }
}
