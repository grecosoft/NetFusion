using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NetFusion.Bootstrap.Container;

namespace Service.WebApi
{
    public class TestHostedService : IHostedService
    {
        private readonly ICompositeContainer _container;
        
        public TestHostedService(ICompositeContainer container)
        {
            _container = container;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            ((IBuiltContainer)_container).Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _container.Stop();
            return Task.CompletedTask;
        }
    }
}