using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Xml.XPath;
using NetFusion.Rest.Docs.Plugin;

namespace NetFusion.Rest.Docs.XmlDescriptions
{
    public class XmlCommentService : IXmlCommentService
    {
        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        private const string ParamXPath = "param[@name='{0}']";
        private const string SummaryTag = "summary";

        private readonly IDocModule _docModule;
        private readonly ConcurrentDictionary<Assembly, XPathNavigator> _xmlAssemblyComments;

        public XmlCommentService(IDocModule docModule)
        {
            _docModule = docModule;
            _xmlAssemblyComments = new ConcurrentDictionary<Assembly, XPathNavigator>();
        }

        public XPathNavigator GetXmlCommentsForTypesAssembly(Type containedType)
        {
            if (containedType is null)
            {
                throw new ArgumentNullException(nameof(containedType));
            }

            Assembly typesAssembly = containedType.Assembly;

            return _xmlAssemblyComments.GetOrAdd(typesAssembly, assembly =>
                 typesAssembly.GetXmlCommentDoc(_docModule.RestDocConfig.DescriptionDirectory)
            );
        }

        public XPathNavigator GetMethodNode(MethodInfo methodInfo)
        {
            if (methodInfo is null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            XPathNavigator xmlCommentsDoc = GetXmlCommentsForTypesAssembly(methodInfo.DeclaringType);

            string methodMemberName = UtilsXmlComment.GetMemberNameForMethod(methodInfo);
            return xmlCommentsDoc.SelectSingleNode(string.Format(MemberXPath, methodMemberName));
        }

        public string GetMethodSummary(MethodInfo methodInfo)
        {
            if (methodInfo is null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            XPathNavigator memberNode = GetMethodNode(methodInfo);

            var summaryNode = memberNode?.SelectSingleNode(SummaryTag);
            return summaryNode != null ? UtilsXmlCommentText.Humanize(summaryNode.InnerXml) : string.Empty;
        }

        public string GetMethodParamComment(XPathNavigator methodNode, string paramName)
        {
            XPathNavigator paramNode = methodNode.SelectSingleNode(string.Format(ParamXPath, paramName));
            return paramNode != null ? UtilsXmlCommentText.Humanize(paramNode.InnerXml) : string.Empty;
        }
    }
}
