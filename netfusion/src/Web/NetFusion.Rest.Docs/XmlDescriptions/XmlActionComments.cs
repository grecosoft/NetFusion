using System;
using System.Xml.XPath;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.XmlDescriptions
{
    public class XmlActionComments : IActionDescription
    {
        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        private const string SummaryTag = "summary";

        public DescriptionContext Context { get; set; }

        public void Describe(ApiActionDoc actionDoc, ApiActionMeta actionMeta)
        {
            Type controllerType = actionMeta.ActionMethodInfo.DeclaringType;

            XPathNavigator xmlCommentDoc = Context.TypeComments
                .GetXmlCommentsForTypesAssembly(controllerType);

            SetActionDescription(xmlCommentDoc, actionDoc, actionMeta);
        }

        private void SetActionDescription(XPathNavigator xmlCommentDoc, ApiActionDoc actionDoc, ApiActionMeta actionMeta)
        {
            string methodMemberName = UtilsXmlComment.GetMemberNameForMethod(actionMeta.ActionMethodInfo);

            // Determine if there are XML comments for the action method:
            XPathNavigator memberNode = xmlCommentDoc.SelectSingleNode(string.Format(MemberXPath, methodMemberName));
            if (memberNode == null) return;

            // Store for reference by other description implementations.
            Context.Properties["xml-member-node"] = memberNode;

            // Determine if the action method has a summary:
            var summaryNode = memberNode.SelectSingleNode(SummaryTag);

            actionDoc.Description = summaryNode != null ? UtilsXmlCommentText.Humanize(summaryNode.InnerXml)
                : string.Empty;
        }
    }
}