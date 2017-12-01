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
        private TestTypeResolver _resolver;
        private AppContainer _container;

        /// <summary>
        /// Returns a new test fixture with a created application container that
        /// can be arranged for testing a specific scenario.
        /// </summary>
        private static ContainerFixture CreateTestFixture()
        {
            var resolver = new TestTypeResolver();

            return new ContainerFixture
            {
                _resolver = resolver,
                _container = new AppContainer(resolver, setGlobalReference: false)
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
        /// Creates a new test fixture for testing an application container.  The created
        /// instance is passed to the provided fixture method used to execute the unit-test.
        /// Once the fixture method completed, the test container is disposed.
        /// </summary>
        /// <param name="fixture">Method specified by the unit-test to execute logic against
        /// a created test-fixture instance.</param>
        public static async Task TestAsync(Func<ContainerFixture, Task> fixture)
        {
            var testFixture = CreateTestFixture();

            if (fixture == null) throw new ArgumentNullException(nameof(fixture),
                "Test fixture cannot be null.");

            await fixture(testFixture);

            testFixture._container.Dispose();
        }

        /// <summary>
        /// Allows the unit-test to arrange the type-resolver and application container under test.  
        /// The type-resolver basically abstracts the logic for loading plug-in types without being 
        /// dependent on .NET assemblies.  This makes unit-testing much easier.
        /// </summary>
        public ContainerArrange Arrange => new ContainerArrange(_resolver, _container);
    }
}
