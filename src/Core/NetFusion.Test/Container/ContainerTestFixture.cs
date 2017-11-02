using NetFusion.Bootstrap.Container;
using NetFusion.Test.Plugins;
using System;

namespace NetFusion.Test.Container
{
    public class ContainerFixture 
    {
        private TestTypeResolver _resolver;
        private AppContainer _container;

        public static ContainerFixture Instance
        {
            get
            {
                var resolver = new TestTypeResolver();

                return new ContainerFixture
                {
                    _resolver = resolver,
                    _container = new AppContainer(resolver, setGlobalReference: false)
                };
            }
        }

        public static void Test(Action<ContainerFixture> fixture)
        {
            var testFixture = Instance;

            if (fixture == null)
                throw new ArgumentNullException(nameof(fixture), "Test fixture cannot be null.");

            fixture(testFixture);

            testFixture._container.Dispose();           
        }

        public ContainerArrange Arrange => new ContainerArrange(_resolver, _container);
    }
}
