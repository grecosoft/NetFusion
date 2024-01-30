namespace NetFusion.Services.UnitTests.Mapping.Entities;

public class Car(string make, string model, string color, int year)
{
    public string Make { get; } = make;
    public string Model { get; } = model;
    public string Color { get; } = color;
    public int Year { get; set; } = year;
}