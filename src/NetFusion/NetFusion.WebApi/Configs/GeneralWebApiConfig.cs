using NetFusion.Bootstrap.Container;

namespace NetFusion.WebApi.Configs
{
    public class GeneralWebApiConfig : IContainerConfig
    {
        public bool UseHttpAttributeRoutes { get; set; } = true;
        public bool UseCamalCaseJson { get; set; } = true;
        public bool UseAutofacFilters { get; set; } = false;
        public bool UseJwtSecurityToken { get; set; } = false;
    }
}
