using System;

namespace NetFusion.Web.Mvc.Metadata.Core
{
    /// <summary>
    /// Metadata returned to the client containing information about
    /// a defined API Group and its associated Action routes. 
    public class ApiGroupMeta
    {
        /// <summary>
        /// The name assigned to the group of related action descriptors.
        /// </summary>
        /// <returns>Assigned group name.</returns>
        public string GroupName { get; }

        /// <summary>
        /// Collection of Action descriptors contained within the group.
        /// </summary>
        /// <returns>Collection of action descriptors.</returns>
        public ApiActionMeta[] Actions { get; }

        public ApiGroupMeta(string groupName, 
            ApiActionMeta[] actions)
        {
            if (string.IsNullOrWhiteSpace(groupName)) throw new ArgumentException(nameof(groupName), 
                "Group name cannot be null or empty string.");
            
            GroupName = groupName;
            Actions = actions ?? 
                throw new ArgumentNullException(nameof(actions));
        }
    }
}