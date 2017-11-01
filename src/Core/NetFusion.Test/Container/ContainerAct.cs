using NetFusion.Bootstrap.Container;
using System;
using System.Threading.Tasks;

namespace NetFusion.Test.Container
{
    public class ContainerAct
    {
        private AppContainer _container;
        private bool _actedOn = false;
        private Exception _resultingException;

        public ContainerAct(AppContainer container)
        {
            _container = container;
        }

        public ContainerAct OnContainer(Action<IAppContainer> act)
        {
            if (_actedOn)
            {
                throw new InvalidOperationException("The container can only be acted on once.");
            }

            _actedOn = true;
            try
            {
                act(_container);
            }
            catch (Exception ex)
            {
                _resultingException = ex;
            }

            return this;
        }

        public async Task<ContainerAct> OnContainer(Func<IAppContainer, Task> act)
        {
            if (_actedOn)
            {
                throw new InvalidOperationException("The container can only be acted on once.");
            }

            _actedOn = true;
            try
            {
                await act(_container);
            }
            catch (Exception ex)
            {
                _resultingException = ex;
            }

            return this;
        }

        public ContainerAssert Assert => new ContainerAssert(_container, _resultingException);

    }
}
