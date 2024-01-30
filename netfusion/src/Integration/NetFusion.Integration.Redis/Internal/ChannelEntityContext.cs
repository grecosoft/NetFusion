using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.Redis.Plugin;

namespace NetFusion.Integration.Redis.Internal;

public class ChannelEntityContext(IPlugin hostPlugin, IServiceProvider serviceProvider)
    : BusEntityContext(hostPlugin, serviceProvider)
{
    public IConnectionModule ConnectionModule { get; } = serviceProvider.GetRequiredService<IConnectionModule>();
}