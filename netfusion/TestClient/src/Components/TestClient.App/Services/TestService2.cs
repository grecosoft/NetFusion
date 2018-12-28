namespace Claims.Notes.App.Services
{
    using TestClient.Domain.Services;

    public class TestService2 : ITestService
    {
        public string GetValue()
        {
            return "Default value.";
        }
    }
}