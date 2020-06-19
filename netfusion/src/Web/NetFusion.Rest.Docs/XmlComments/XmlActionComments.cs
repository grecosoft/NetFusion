using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.XPath;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.XmlComments
{
    public class XmlActionComments : IActionDescription
    {
        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        private const string SummaryTag = "summary";

        public IDictionary<string, object> Context { get; set; }

        public void Describe(ApiActionDoc actionDoc, ApiActionMeta actionMeta)
        {
            Assembly declaringAssembly = actionMeta.ActionMethodInfo.DeclaringType?.Assembly;
            if (declaringAssembly == null)
            {
                // TODO:  throw
            }
            
            XPathNavigator xmlCommentDoc = declaringAssembly.GetXmlCommentDoc(AppContext.BaseDirectory);
            string methodMemberName = UtilsXmlComment.GetMemberNameForMethod(actionMeta.ActionMethodInfo);
            
            // Determine if there are XML comments for the action method:
            XPathNavigator memberNode = xmlCommentDoc.SelectSingleNode(string.Format(MemberXPath, methodMemberName));
            if (memberNode == null) return;

            // Store for reference by other description implementations.
            Context["xml-member-node"] = memberNode;
            
            // Determine if the action method has a summary:
            var summaryNode = memberNode.SelectSingleNode(SummaryTag);

            actionDoc.Description = summaryNode != null ? UtilsXmlCommentText.Humanize(summaryNode.InnerXml) 
                : string.Empty;
        }
    }
}