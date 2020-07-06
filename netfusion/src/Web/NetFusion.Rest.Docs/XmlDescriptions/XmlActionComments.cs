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
            if (methodNode == null)
            {
                return;
            }

            ApplyParamDocs(methodNode, actionDoc.RouteParams, actionMeta.RouteParameters);
            ApplyParamDocs(methodNode, actionDoc.HeaderParams, actionMeta.HeaderParameters);
            ApplyParamDocs(methodNode, actionDoc.QueryParams, actionMeta.QueryParameters);
        }

        private void ApplyParamDocs(
            XPathNavigator methodNode,
            ICollection<ApiParameterDoc> paramDocs,
            IEnumerable<ApiParameterMeta> paramsMetaItems)
        {
            foreach (ApiParameterMeta paramMeta in paramsMetaItems)
            {
                var paramDoc = new ApiParameterDoc
                {
                    Name = paramMeta.BindingName,
                    IsOptional = paramMeta.IsOptional,
                    DefaultValue = paramMeta.DefaultValue,
                    Type = paramMeta.ParameterType.GetJsTypeName(),
                    Description = _xmlComments.GetMethodParamComment(methodNode, paramMeta.ParameterName)
                };

                paramDocs.Add(paramDoc);
            }
        }
    }
}