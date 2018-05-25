namespace Demo.App.Entities
{
    public class Car
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }

        // Don't expose theses on the model :)
        public bool HasSalvageTitle { get; set; }
        public bool WasSmokerCar { get; set;}
    }
}
