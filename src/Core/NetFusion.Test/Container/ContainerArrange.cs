using NetFusion.Bootstrap.Container;
using NetFusion.Test.Plugins;
using System;

namespace NetFusion.Test.Container
{
    public class ContainerArrange
    {
        private TestTypeResolver _resolver;
        private AppContainer _container;

        public ContainerArrange(TestTypeResolver resolver, AppContainer container)
        {
            _resolver = resolver;
            _container = container;
        }

        public ContainerArrange Resolver(Action<TestTypeResolver> arrange)
        {
            arrange(_resolver);
            return this;
        }

        public ContainerArrange Container(Action<IAppContainer> arrange)
        {
            arrange(_container);
            return this;
        }

        public ContainerAct Act => new ContainerAct(_container);
    }
}

