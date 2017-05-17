using System;

namespace NetFusion.Web.Mvc.Metadata
{
    /// <summary>
    /// Applied to a controller to include metadata about itself and any routes
    /// marked with the ActionMetaAttribute attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class GroupMetaAttribute : Attribute
    {
        /// <summary>
        /// The name of the group to associate with the controller.  If not specified,
        /// the name of the controller is used as the group name. 
        /// </summary>
        /// <returns>Name of group within returned metadata.</returns>
        public string GroupName { get; }

        /// <summary>
        ///  Assigns group name.
        /// </summary>
        /// <param name="groupName">Group name.  If not specified, controller name is used.</param>
        public GroupMetaAttribute(string groupName = null) 
        {
            this.GroupName = groupName;
        }
    }
}