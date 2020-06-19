using System.Collections.Generic;
using System.Xml.XPath;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.XmlComments
{
    public class XmlParamComments : IParamDescription
    {
        private const string AttribXPath = "param[@name='{0}']";
        public IDictionary<string, object> Context { get; set; }
        
        public void Describe(ApiParameterDoc paramDoc, ApiParameterMeta paramMeta)
        { 
            var actionNode = this.GetContextItem<XPathNavigator>("xml-member-node");
            XPathNavigator paramNode = actionNode.SelectSingleNode(string.Format(AttribXPath, paramMeta.ParameterName));  // TODO... expose paraminfo...
            
            paramDoc.Description = paramNode != null ? UtilsXmlCommentText.Humanize(paramNode.InnerXml) 
                : string.Empty;
        }
    }
}