using NetFusion.Common.Extensions.Reflection;
using NetFusion.Rest.Resources.Doc;
using System;
using System.Linq;
using System.Reflection;

namespace NetFusion.Rest.Server.Documentation.Core
{
    /// <summary>
    /// Contains the documentation for a specific controller's action method.
    /// </summary>
    public class DocAction
    {
        /// <summary>
        /// A unique value identifying the action.
        /// </summary>
        public string ActionId { get; private set; }

        /// <summary>
        /// The embedded resource names specified on action.
        /// </summary>
        public ActionEmbeddedName[] EmbeddedNames { get; private set; }

        /// <summary>
        /// The method information for the action method.
        /// </summary>
        public MethodInfo ActionMethodInfo { get; }
  
        private Type _responseResourceType;

        public DocAction(MethodInfo actionMethodInfo)
        {
            ActionMethodInfo = actionMethodInfo ?? 
                throw new ArgumentNullException(nameof(actionMethodInfo), "Action name not specified.");

            SetAttributeSpecifiedValues(actionMethodInfo);
        }

        private void SetAttributeSpecifiedValues(MethodInfo actionMethodInfo)
        {
            var actionDocAttrib = actionMethodInfo.GetAttribute<DocActionAttribute>();
            if (actionDocAttrib != null)
            {
                ActionId = actionDocAttrib.Id;

                HttpCodes = actionDocAttrib.StatusCodes
                    .Select(c => new DocHttpCode(c))
                    .ToArray();

                ResponseResourceType = actionDocAttrib.ResponseResourceType;
            }

            EmbeddedNames = actionMethodInfo.GetEmbeddedNames();     
        }
        
        // Action descriptions:
        public string Description { get; set; }
        public string ResponseDescription { get; set; }

        // Associated action documentation:
        public DocActionParam[] ActionParams { get; set; }
        public DocHttpCode[] HttpCodes { get; set; }
        public DocRelation[] Relations { get; set; }
        public DocEmbeddedResource[] EmbeddedResources { get; set; }

        // Optional resource type received by action method when invoked.
        public Type RequestResourceType { get; set; }

        // The resource type returned from the action method.  If the return type of the action
        // method was specified with the DocAction attribute, this type should be used.  Otherwise,
        // the consumer of the class is responsible for setting the resource type.  This is for the
        // case where an action method returns a resource but the return type is IActionResult.
        public Type ResponseResourceType 
        {
            set => _responseResourceType = _responseResourceType ?? value;
            get => _responseResourceType;
        }
    }   
}

