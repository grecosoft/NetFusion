using NetFusion.Bootstrap.Container;
using NetFusion.Common;
using System;

namespace NetFusion.Bootstrap.Testing
{
    /// <summary>
    /// Allows client to act on an instance of a container.  This class
    /// implements the common TDD style of arrange, act, and assert.
    /// </summary>
    public class ContainerAct
    {
        private readonly AppContainer _container;

        public ContainerAct(AppContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// Provides an action that is passed the instance of the
        /// application container.
        /// </summary>
        /// <param name="act">Delegate provided by the consumer that can
        /// act on the application container.</param>
        /// <returns>Returns an instance of a class that can be used
        /// to assert that state of the application container after
        /// the action has been taken.</returns>
        public ContainerAssert Act(Action<AppContainer> act)
        {
            Check.NotNull(act, nameof(act), "action delegate not specified");
            try
            {
                act(_container);
            }
            catch (Exception ex)
            {
                return new ContainerAssert(_container, ex);
            }

            return new ContainerAssert(_container, null);
        }
    }
}
