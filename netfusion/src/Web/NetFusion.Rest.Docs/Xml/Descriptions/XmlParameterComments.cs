using System;
using System.Xml.XPath;
using NetFusion.Rest.Docs.Core.Descriptions;
using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Xml.Descriptions
{
    /// <summary>
    /// Adds additional documentation to an action's parameter.  An action's parameter can be for
    /// for a Route, Header, Query, or Body parameter.
    /// </summary>
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
            
            parameterDoc.Description = _xmlComments.GetMethodParamComment(methodNode, parameterMeta.ParameterName);
        }
    }
}