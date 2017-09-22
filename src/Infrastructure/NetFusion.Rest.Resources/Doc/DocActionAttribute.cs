using System;
using System.Net;

namespace NetFusion.Rest.Resources.Doc
{
    /// <summary>
    /// Used to specify action related document information used as inputs to
    /// the documentation generation process.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class DocActionAttribute : Attribute
    {
        /// <summary>
        /// Uniquely identifies the action.  Used to reference related documentation
        /// specified within an XML file.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The possible status codes returned from the action.
        /// </summary>
        public HttpStatusCode[] StatusCodes { get; } = new HttpStatusCode[] { };

        /// <summary>
        /// The response resource type.  Not required if the action method returns the 
        /// resource type or a task of the resource type.
        /// </summary>
        public Type ResponseResourceType { get; }

        /// <summary>
        /// Specifies information used by the action documentation generation process.
        /// </summary>
        /// <param name="id">Uniquely identifies the action.</param>
        /// <param name="statusCodes">The possible status codes returned from the action.</param>
        /// <param name="responseResourceType">The response resource type.  Not required if
        /// the action method returns the resource type or a task of the resource type.</param>
        public DocActionAttribute(string id,
            HttpStatusCode[] statusCodes = null,            
            Type responseResourceType = null) 
        {
            Id = id ?? throw new ArgumentNullException(nameof(id), "Action Documentation Identifier not specified.");

            StatusCodes = statusCodes;
            ResponseResourceType = responseResourceType;
        }
    }
}
