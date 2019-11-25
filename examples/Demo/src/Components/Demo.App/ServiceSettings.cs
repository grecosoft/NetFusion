using System.Linq;
using NetFusion.Base.Validation;
using NetFusion.Settings;

namespace Demo.App
{
    [ConfigurationSection("microservice:database")]
    public class ServiceSettings : IAppSettings,
        IValidatableType
    {
        public ServiceSettings()
        {
            Ports = new int[] {};
        }

        public string Name { get; set; }
        public string Url { get; set; }
        public int[] Ports { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public void Validate(IObjectValidator validator)
        {
            validator.Verify(!string.IsNullOrWhiteSpace(Name), "Name not specified.");
            validator.Verify(!string.IsNullOrWhiteSpace(Password), "Password not specified.");
            validator.Verify(Ports.All(p => p > 8000), "All port must be greater than 8000.");
        }
    }
}
