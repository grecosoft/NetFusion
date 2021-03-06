using System.Threading.Tasks;

namespace NetFusion.Bootstrap.Health
{
    /// <summary>
    /// Implemented by any IPluginModule derive class to participate in
    /// determining the overall health of the composite-application. 
    /// </summary>
    public interface IModuleHealthCheck
    {
        /// <summary>
        /// Populates a module health-check by recording the health of specific
        /// aspects managed by the module.
        /// </summary>
        /// <param name="healthCheck">Health check to be populated.</param>
        Task CheckModuleAspectsAsync(ModuleHealthCheck healthCheck);
    }
}