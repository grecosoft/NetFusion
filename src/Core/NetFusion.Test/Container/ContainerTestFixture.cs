using NetFusion.Bootstrap.Container;
using NetFusion.Test.Plugins;
using System;

namespace NetFusion.Test.Container
{
    public class ContainerTestFixture : IDisposable
    {
        private TestTypeResolver _resolver;
        private AppContainer _container;

        public static ContainerTestFixture Instance
        {
            get
            {
                var resolver = new TestTypeResolver();

                return new ContainerTestFixture
                {
                    _resolver = resolver,
                    _container = new AppContainer(resolver, setGlobalReference: false)
                };
            }
        }

        public ContainerArrange Arrange => new ContainerArrange(_resolver, _container);

        public void Dispose()
        {
            _container?.Dispose();
        }
    }
}
