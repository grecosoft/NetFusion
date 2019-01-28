namespace Service.Infra
{
    using NetFusion.Base;
    using NetFusion.Redis.Publisher;
    using Service.Domain.Events;

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