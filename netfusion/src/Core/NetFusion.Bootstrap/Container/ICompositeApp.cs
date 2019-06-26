using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace NetFusion.Bootstrap.Container
{
    public interface ICompositeApp
    {
        
        IDictionary<string, object> Log { get; }
        
        /// <summary>
        /// Starts each plugin module.
        /// </summary>
        /// <returns>Task that can be awaited after which all plugin modules will have been started.</returns>
        Task StartAsync();

        IServiceScope CreateServiceScope();
        
        /// <summary>
        /// Stops each plugin module.
        /// </summary>
        /// <returns>Task that cna be awaited after which all plugin modules will have been stopped.</returns>
        Task StopAsync();
    }
}