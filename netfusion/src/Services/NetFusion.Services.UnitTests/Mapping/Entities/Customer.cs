namespace NetFusion.Services.UnitTests.Mapping.Entities;

public class Customer(string firstName, string lastName, int age)
{
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;
    public int Age { get; } = age;
}