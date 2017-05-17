using NetFusion.Web.Mvc.Metadata.Core;

namespace NetFusion.Web.Mvc.Metadata
{
    /// <summary>
    /// Returns metadata for controller and
    /// </summary>
    public interface IApiMetadataService
    {
        /// <summary>
        /// Returns route metadata for all defined groups.
        /// </summary>
        /// <returns>Metadata describing API group and actions.</returns>
        ApiGroupMeta[] GetApiGroups();

        /// <summary>
        /// Returns route metadata for a given API group.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <returns>Metadata describing API group and actions.</returns>
        ApiGroupMeta[] GetApiGroup(string groupName);
    }
}