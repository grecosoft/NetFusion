using System.Collections.Generic;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;
using NetFusion.Common.Extensions.Types;
using System.Xml.XPath;

namespace NetFusion.Rest.Docs.XmlDescriptions
{
    /// <summary>
    /// Sets the comments associated with a given Web Controller's action method.
    /// </summary>
    public class XmlActionComments : IActionDescription
    {
        private readonly IXmlCommentService _xmlComments;

        public XmlActionComments(IXmlCommentService xmlComments)
        {
            _xmlComments = xmlComments ?? throw new System.ArgumentNullException(nameof(xmlComments));
        }

        public void Describe(ApiActionDoc actionDoc, ApiActionMeta actionMeta)
        {
            actionDoc.Description = _xmlComments.GetMethodComments(actionMeta.ActionMethodInfo);

            XPathNavigator methodNode = _xmlComments.GetMethodNode(actionMeta.ActionMethodInfo);

            ApplyParamDocs(actionDoc.RouteParams, actionMeta.RouteParameters, methodNode);
            ApplyParamDocs(actionDoc.HeaderParams, actionMeta.HeaderParameters, methodNode);
            ApplyParamDocs(actionDoc.QueryParams, actionMeta.QueryParameters, methodNode);
        }

        // Creates and adds parameter documentation for a method's parameters. If no XML node
        // was found for the method, the passed methodNode will be null.  In this case, all
        // information is added besides any XML comments.
        private void ApplyParamDocs(ICollection<ApiParameterDoc> paramDocs,
            IEnumerable<ApiParameterMeta> paramsMetaItems, 
            XPathNavigator methodNode = null)
        {
            foreach (ApiParameterMeta paramMeta in paramsMetaItems)
            {
                var paramDoc = new ApiParameterDoc
                {
                    Name = paramMeta.BindingName,
                    IsOptional = paramMeta.IsOptional,
                    DefaultValue = paramMeta.DefaultValue?.ToString(),
                    Type = paramMeta.ParameterType.GetJsTypeName()
                };
                
                if (methodNode != null)
                {
                    paramDoc.Description = _xmlComments.GetMethodParamComment(methodNode, paramMeta.ParameterName);
                }

                paramDocs.Add(paramDoc);
            }
        }
    }
}