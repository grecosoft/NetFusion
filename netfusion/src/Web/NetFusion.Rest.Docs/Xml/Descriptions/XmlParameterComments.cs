using System;
using System.Xml.XPath;
using NetFusion.Common.Extensions.Types;
using NetFusion.Rest.Docs.Core.Descriptions;
using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Xml.Descriptions
{
    public class XmlParameterComments : IParameterDescription
    {
        private readonly IXmlCommentService _xmlComments;
        
        public XmlParameterComments(IXmlCommentService xmlComments)
        {
            _xmlComments = xmlComments ?? throw new ArgumentNullException(nameof(xmlComments));
        }
        
        public void Describe(ApiParameterDoc parameterDoc, ApiActionMeta actionMeta, ApiParameterMeta parameterMeta)
        {
            XPathNavigator methodNode = _xmlComments.GetMethodNode(actionMeta.ActionMethodInfo);
            if (methodNode == null)
            {
                return;
            }
            
            parameterDoc.Type = parameterMeta.ParameterType.GetJsTypeName();
            parameterDoc.Description = _xmlComments.GetMethodParamComment(methodNode, parameterMeta.ParameterName);
        }
    }
}