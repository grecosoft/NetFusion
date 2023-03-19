using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.Redis.Plugin;

namespace NetFusion.Integration.Redis.Internal;

public class ChannelEntityContext : BusEntityContext
{
    public IConnectionModule ConnectionModule { get; }
    
    public ChannelEntityContext(IPlugin hostPlugin, IServiceProvider serviceProvider) :
        base(hostPlugin, serviceProvider)
    {
        ConnectionModule = serviceProvider.GetRequiredService<IConnectionModule>();
    }
}