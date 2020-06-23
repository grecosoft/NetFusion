using System;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using NetFusion.Common.Extensions.Types;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Models;

namespace NetFusion.Rest.Docs.XmlDescriptions
{
    public class XmlTypeCommentService : ITypeCommentService
    {
        private const string TypeMemberSummaryXPath = "/doc/members/member[@name='{0}']/summary";
        
        public ApiResourceDoc GetResourceDoc(Type resourceType)
        {
            Assembly declaringAssembly = resourceType.Assembly;
            var resourceDoc = new ApiResourceDoc();

            XPathNavigator xmlCommentDoc = declaringAssembly.GetXmlCommentDoc(AppContext.BaseDirectory);
            if (xmlCommentDoc == null)
            {
                return resourceDoc;
            }

            resourceDoc.Description = GetTypeComment(xmlCommentDoc, resourceType);
            foreach (PropertyInfo propInfo in resourceType.GetProperties().Where(p => p.CanRead))
            {
                var propDoc = new ApiPropertyDoc
                {
                    Name = propInfo.Name,
                    Type = propInfo.PropertyType.GetJsTypeName(),
                    Description = GetTypeMemberComment(xmlCommentDoc, propInfo)
                };
                
                resourceDoc.Properties.Add(propDoc);
            }
            
            return resourceDoc;
        }

        private static string GetTypeComment(XPathNavigator xmlCommentDoc, Type resourceType)
        {
            string typeMemberName = UtilsXmlComment.GetMemberNameForType(resourceType);
            
            XPathNavigator memberNode = xmlCommentDoc.SelectSingleNode(string.Format(TypeMemberSummaryXPath, typeMemberName));
            return memberNode == null ? string.Empty : UtilsXmlCommentText.Humanize(memberNode.InnerXml);
        }

        private static string GetTypeMemberComment(XPathNavigator xmlCommentDoc, MemberInfo propertyInfo)
        {
            string propMemberName = UtilsXmlComment.GetMemberNameForFieldOrProperty(propertyInfo);
            
            XPathNavigator memberNode = xmlCommentDoc.SelectSingleNode(string.Format(TypeMemberSummaryXPath, propMemberName));
            return memberNode == null ? string.Empty : UtilsXmlCommentText.Humanize(memberNode.InnerXml);
        }
    }
}