using Demo.Domain.Events;
using NetFusion.Base;
using NetFusion.Redis.Publisher;

namespace Demo.Infra
{
    public class ChannelRegistry : ChannelRegistryBase
    {
        protected override void OnRegister()
        {
            AddChannel<OrderSubmitted>("testdb", "Orders")
                .SetEventData(o => $"{o.PartNumber}.{o.State}")
                .AppliesWhen(o => o.Quantity > 5)
                .UseContentType(ContentTypes.MessagePack);
        }
    }
}
