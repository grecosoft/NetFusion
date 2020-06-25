using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using NetFusion.Common.Extensions.Types;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Models;
using NetFusion.Rest.Resources;

namespace NetFusion.Rest.Docs.XmlDescriptions
{
    public class XmlTypeCommentService : ITypeCommentService
    {
        private const string TypeMemberSummaryXPath = "/doc/members/member[@name='{0}']/summary";
        private static Type[] PrimitiveTypes { get; } = {typeof(string), typeof(DateTime)};
        
        public ApiResourceDoc GetResourceDoc(Type resourceType)
        {
            Assembly declaringAssembly = resourceType.Assembly;
            XPathNavigator xmlCommentDoc = declaringAssembly.GetXmlCommentDoc(AppContext.BaseDirectory);
            
            return xmlCommentDoc == null ? null : BuildResourceDoc(xmlCommentDoc, resourceType);
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

        private ApiResourceDoc BuildResourceDoc(XPathNavigator xmlCommentDoc, Type resourceType)
        {
            var resourceDoc = new ApiResourceDoc
            {
                Description = GetTypeComment(xmlCommentDoc, resourceType),
                ResourceName = resourceType.GetExposedResourceName()
        };

            foreach (PropertyInfo propInfo in GetDescribableProperties(resourceType))
            {
                var propDoc = new ApiPropertyDoc
                {
                    Name = propInfo.Name,
                    Description = GetTypeMemberComment(xmlCommentDoc, propInfo)
                    
                };
                
                if (IsPrimitiveType(propInfo))
                {
                    propDoc.Type = propInfo.PropertyType.GetJsTypeName();
                    
                }
                else if (propInfo.PropertyType.IsClass)
                {
                    propDoc.Type = BuildResourceDoc(xmlCommentDoc, propInfo.PropertyType);
                }
                
                resourceDoc.Properties.Add(propDoc);
            }

            return resourceDoc;
        }

        private static IEnumerable<PropertyInfo> GetDescribableProperties(Type type) => 
            type.GetProperties()
            .Where(p => p.CanRead);

        private static bool IsPrimitiveType(PropertyInfo propertyInfo) => 
            propertyInfo.PropertyType.IsPrimitive || PrimitiveTypes.Contains(propertyInfo.PropertyType);
    }
}