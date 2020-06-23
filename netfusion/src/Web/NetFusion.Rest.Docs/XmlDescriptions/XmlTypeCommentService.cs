using System;
using System.Reflection;
using System.Xml.XPath;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Models;

namespace NetFusion.Rest.Docs.XmlDescriptions
{
    public class XmlTypeCommentService : ITypeCommentService
    {
        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        
        public ApiResourceDoc GetResourceDoc(Type resourceType)
        {
            Assembly declaringAssembly = resourceType.Assembly;

            XPathNavigator xmlCommentDoc = declaringAssembly.GetXmlCommentDoc(AppContext.BaseDirectory);
            string methodMemberName = UtilsXmlComment.GetMemberNameForType(resourceType);
            
            throw new NotImplementedException();
        }
    }
}