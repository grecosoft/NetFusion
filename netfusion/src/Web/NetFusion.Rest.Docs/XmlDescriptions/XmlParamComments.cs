using System.Collections.Generic;
using System.Xml.XPath;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.XmlDescriptions
{
    public class XmlParamComments : IParamDescription
    {
        private const string AttribXPath = "param[@name='{0}']";
        public DescriptionContext Context { get; set; }
        
        public void Describe(ApiParameterDoc paramDoc, ApiParameterMeta paramMeta)
        { 
            var actionNode = this.GetContextItem<XPathNavigator>("xml-member-node");
            XPathNavigator paramNode = actionNode.SelectSingleNode(string.Format(AttribXPath, paramMeta.ParameterName)); 
            
            paramDoc.Description = paramNode != null ? UtilsXmlCommentText.Humanize(paramNode.InnerXml) 
                : string.Empty;
        }
    }
}