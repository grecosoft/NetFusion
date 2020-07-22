using System;
using System.Collections.Generic;
using System.Xml.XPath;
using NetFusion.Common.Extensions.Types;
using NetFusion.Rest.Docs.Core;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.XmlComments
{
    /// <summary>
    /// Sets the comments associated with a given Web Controller's action method.
    /// </summary>
    public class XmlActionComments : IActionDescription
    {
        private readonly IXmlCommentService _xmlComments;
        private readonly ITypeCommentService _typeComments;

        public XmlActionComments(
            IXmlCommentService xmlComments, 
            ITypeCommentService typeComments)
        {
            _xmlComments = xmlComments ?? throw new ArgumentNullException(nameof(xmlComments));
            _typeComments = typeComments ?? throw new ArgumentNullException(nameof(typeComments));
        }

        public void Describe(ApiActionDoc actionDoc, ApiActionMeta actionMeta)
        {
            actionDoc.Description = _xmlComments.GetMethodComments(actionMeta.ActionMethodInfo);

            XPathNavigator methodNode = _xmlComments.GetMethodNode(actionMeta.ActionMethodInfo);

            ApplyParamDocs(actionDoc.RouteParams, actionMeta.RouteParameters, methodNode);
            ApplyParamDocs(actionDoc.HeaderParams, actionMeta.HeaderParameters, methodNode);
            ApplyParamDocs(actionDoc.QueryParams, actionMeta.QueryParameters, methodNode);
            ApplyParamDocs(actionDoc.BodyParams, actionMeta.BodyParameters, methodNode);
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
                    Name = paramMeta.ParameterName,
                    IsOptional = paramMeta.IsOptional,
                    DefaultValue = paramMeta.DefaultValue?.ToString(),
                    Type = paramMeta.ParameterType.GetJsTypeName()
                };
                
                if (methodNode != null && ReflectionUtil.IsPrimitiveType(paramMeta.ParameterType) )
                {
                    paramDoc.Description = _xmlComments.GetMethodParamComment(methodNode, paramMeta.ParameterName);
                }
                else if (paramMeta.ParameterType.IsClass)
                {
                    paramDoc.ResourceDoc = _typeComments.GetResourceDoc(paramMeta.ParameterType);
                }
                
                paramDocs.Add(paramDoc);
            }
        }
    }
}