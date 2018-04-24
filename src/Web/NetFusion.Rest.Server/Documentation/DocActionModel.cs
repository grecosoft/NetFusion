using NetFusion.Rest.Server.Documentation.Core;
using System;

namespace NetFusion.Rest.Server.Documentation
{
    /// <summary>
    /// Model containing the documentation for a given action method returned to the requesting client.
    /// </summary>
    public class DocActionModel
    {
        public DocActionModel(DocAction actionDoc)
        {
            if (actionDoc == null) throw new ArgumentNullException(nameof(actionDoc),
                "Action Documentation cannot be null.");

            ActionId = actionDoc.ActionId;
            Description = actionDoc.Description;
            RouteParams = actionDoc.ActionParams;
            ErrorCodes = actionDoc.HttpCodes;
            EmbeddedResources = actionDoc.EmbeddedResources;
            ResponseDescription = actionDoc.ResponseDescription;
            Relations = actionDoc.Relations;
        }

        /// <summary>
        /// Unique value identifying the action method.
        /// </summary>
		public string ActionId { get; }

        /// <summary>
        /// Description of the action method's behavior.
        /// </summary>
		public string Description { get; }

        /// <summary>
        /// Documentation for each of the route's action parameters.
        /// </summary>
		public DocActionParam[] RouteParams { get; }

        /// <summary>
        /// Documentation for the possible error codes that can be returned.
        /// </summary>
		public DocHttpCode[] ErrorCodes { get; }

        /// <summary>
        /// Documentation for each of the embedded resources.  Provides a description
        /// for the key identifying the type of embedded resource.
        /// </summary>
        public DocEmbeddedResource[] EmbeddedResources { get; }

        /// <summary>
        /// Description of the response returned from the action method.
        /// </summary>
        public string ResponseDescription { get; }

		// The following properties are set when the documentation is requested.

        /// <summary>
        /// Documentation for each of the link-relations associated with returned resource.
        /// </summary>
        public DocRelation[] Relations { get; set; }

        /// <summary>
        /// Provides documentation about a resource that can be posted to the action.  Only
        /// applies if the action method accepts a resource type parameter.
        /// </summary>
        public DocResource ReqestResource { get; set; }

        /// <summary>
        /// Provides documentation about the resource returned from the action method.  Only
        /// applies if the action method returns a resource type.
        /// </summary>
        public DocResource ResponseResource { get; set; }

        /// <summary>
        /// The type-script definition if the action method can be posed a resource.
        /// </summary>
        public string RequestTypeScript { get; set; }

        /// <summary>
        /// The type-script definition if the action method returns a resource.
        /// </summary>
        public string ResponseTypeScript { get; set; }
    }
}
