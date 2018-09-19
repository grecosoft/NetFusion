namespace Demo.WebApi
{
    using Domain.Events;
    using NetFusion.Base;
    using NetFusion.Redis.Publisher;
    
    public class ChannelRegistry : ChannelRegistryBase
    {
        protected override void OnRegister()
        {
            AddChannel<AutoPurchasedEvent>("testdb", "auto-purchases")
                .SetEventData(p => $"{p.Make}.{p.Model}.{p.Year}")
                .AppliesWhen(de => de.Make != "Ford")
                .UseContentType(ContentTypes.MessagePack);
            
            AddChannel<OrderSubmitted>("testdb", "Orders")
                .SetEventData(o => $"{o.PartNumber}.{o.State}")
                .AppliesWhen(o => o.Quantity > 5)
                .UseContentType(ContentTypes.MessagePack);
        }
    }
}