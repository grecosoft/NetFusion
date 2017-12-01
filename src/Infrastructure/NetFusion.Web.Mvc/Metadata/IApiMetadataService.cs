using NetFusion.Web.Mvc.Metadata.Core;

namespace NetFusion.Web.Mvc.Metadata
{
    /// <summary>
    /// Returns metadata for controller and its associated routes.
    /// </summary>
    public interface IApiMetadataService
    {
        /// <summary>
        /// Returns route metadata for all defined groups.
        /// </summary>
        /// <returns>Metadata describing API groups and actions.</returns>
        ApiGroupMeta[] GetApiGroups();

        /// <summary>
        /// Returns route metadata for a given API group.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <returns>Metadata describing API group and actions.</returns>
        ApiGroupMeta[] GetApiGroup(string groupName);

        /// <summary>
        /// Returns the route meta for a specific API group and action.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <returns>The route metadata for the action.  If not found,
        /// an exception is thrown.</returns>
        ApiActionMeta GetApiAction(string groupName, string actionName);
    }
}