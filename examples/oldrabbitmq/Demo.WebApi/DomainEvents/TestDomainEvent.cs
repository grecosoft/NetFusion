using NetFusion.Messaging.Types;

namespace Demo.WebApi.DomainEvents
{
    public class TestDomainEvent : DomainEvent
    {
        public string Make { get; }
        public string Model { get; }

        public TestDomainEvent(string make, string model)
        {
            Make = make;
            Model = model;

    
            this.SetRouteKey($"{make}.{model}");
        }
    }
}