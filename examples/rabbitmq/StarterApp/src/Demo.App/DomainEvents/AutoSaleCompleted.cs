using NetFusion.Messaging.Types;

namespace Demo.App.DomainEvents
{
    public class AutoSaleCompleted : DomainEvent
    {
        public string Make { get; }
        public string Model { get; }
        public int Year { get; }
        public string Color { get; }
        public bool IsNew { get; set; } = true;

        public AutoSaleCompleted(
            string make,
            string model,
            int year,
            string color)
        {
            this.SetRouteKey(make, model, year);
            
            Make = make;
            Model = model;
            Year = year;
            Color = color;
        }
    }
}