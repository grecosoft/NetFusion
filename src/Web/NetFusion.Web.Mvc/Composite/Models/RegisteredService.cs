namespace NetFusion.Web.Mvc.Composite.Models
{
    /// <summary>
    /// Model representing a type registered with the DI
    /// container defined within the plugin.
    /// </summary>
    public class RegisteredService
    {
        public string RegisteredType { get; set; }
        public string ServiceType { get; set; }
        public string LifeTime { get; set; }
    }
}
