using System;

namespace NetFusion.Web.Mvc.Metadata
{
    /// <summary>
    /// Applied to a controller route to include metadata within the 
    /// returned group metadata.
    /// </summary> 
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ActionMetaAttribute : Attribute
    {
        /// <summary>
        /// The name of the action returned in the route metadata.
        /// </summary>
        /// <returns>Action name.</returns>
        public string ActionName { get; }

        /// <summary>
        /// The name of the action returned within the metadata.
        /// </summary>
        /// <param name="actionName">Name returned in metadata.</param>
        public ActionMetaAttribute(string actionName)
        {
            if (string.IsNullOrWhiteSpace(actionName))
            {
                throw new ArgumentException("Action name cannot be null or empty string.");
            }
            
            ActionName = actionName;
        }
    }
}