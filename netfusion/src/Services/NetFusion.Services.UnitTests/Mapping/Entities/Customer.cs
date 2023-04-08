namespace NetFusion.Services.UnitTests.Mapping.Entities;

public class Customer
{
    public string FirstName { get; }
    public string LastName { get; }
    public int Age { get; }
        
    public Customer(string firstName, string lastName, int age)
    {
        FirstName = firstName;
        LastName = lastName;
        Age = age;
    }
}