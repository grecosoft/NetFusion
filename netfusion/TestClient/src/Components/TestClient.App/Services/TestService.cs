namespace TestClient.App.Services
{
    using TestClient.Domain.Services;

    public class TestService : ITestService
    {
        public string GetValue()
        {
            return "This is a test.";
        }
    }
}