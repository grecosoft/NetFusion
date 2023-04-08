namespace NetFusion.Services.UnitTests.Mapping.Entities;

public class Car
{
    public string Make { get; } 
    public string Model { get; }
    public string Color { get; }
    public int Year { get; set; }
        
    public Car(string make, string model, string color, int year)
    {
        Make = make;
        Model = model;
        Color = color;
        Year = year;
    }
}