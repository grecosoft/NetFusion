using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Xml.XPath;
using NetFusion.Rest.Docs.Plugin;

namespace NetFusion.Rest.Docs.XmlComments
{
    public class XmlCommentService : IXmlCommentService
    {
        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        private const string ParamXPath = "param[@name='{0}']";

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
                 GetXmlCommentDoc(typesAssembly, _docModule.RestDocConfig.DescriptionDirectory)
            );
        }

        private static XPathNavigator GetXmlCommentDoc(Assembly assembly, string basePath)
        {
            string fileName = Path.Join(basePath, $"{assembly.GetName().Name}.xml");
            return File.Exists(fileName) ? new XPathDocument(fileName).CreateNavigator() : null;
        }

        public XPathNavigator GetTypeNode(Type classType)
        {
            if (classType is null)
            {
                throw new ArgumentNullException(nameof(classType));
            }

            XPathNavigator xmlCommentsDoc = GetXmlCommentsForTypesAssembly(classType);

            string typeMemberName = UtilsXmlComment.GetMemberNameForType(classType);
            return xmlCommentsDoc?.SelectSingleNode(string.Format(MemberXPath, typeMemberName));
        }

        public string GetTypeComments(Type classType)
        {
            XPathNavigator memberNode = GetTypeNode(classType);

            var summaryNode = memberNode?.SelectSingleNode("summary");
            return summaryNode != null ? UtilsXmlCommentText.Humanize(summaryNode.InnerXml) : string.Empty;
        }

        public XPathNavigator GetMethodNode(MethodInfo methodInfo)
        {
            if (methodInfo is null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            XPathNavigator xmlCommentsDoc = GetXmlCommentsForTypesAssembly(methodInfo.DeclaringType);

            string methodMemberName = UtilsXmlComment.GetMemberNameForMethod(methodInfo);
            return xmlCommentsDoc?.SelectSingleNode(string.Format(MemberXPath, methodMemberName));
        }

        public string GetMethodComments(MethodInfo methodInfo)
        {
            XPathNavigator memberNode = GetMethodNode(methodInfo);

            if (memberNode == null)
            {
                return string.Empty;
            }

            var summaryInnerXml = memberNode.SelectSingleNode("summary")?.InnerXml;
            var returnsInnerXml = memberNode.SelectSingleNode("returns")?.InnerXml;

            return (UtilsXmlCommentText.Humanize(summaryInnerXml ?? String.Empty)
                + " " + UtilsXmlCommentText.Humanize(returnsInnerXml ?? String.Empty)).Trim();
        }

        public string GetTypeMemberComments(MemberInfo memberInfo)
        {
            XPathNavigator xmlCommentsDoc = GetXmlCommentsForTypesAssembly(memberInfo.DeclaringType);

            string memberName = UtilsXmlComment.GetMemberNameForFieldOrProperty(memberInfo);
            XPathNavigator memberNode = xmlCommentsDoc?.SelectSingleNode(string.Format(MemberXPath, memberName));

            var summaryNode = memberNode?.SelectSingleNode("summary");
            return summaryNode != null ? UtilsXmlCommentText.Humanize(summaryNode.InnerXml) : string.Empty;
        }

        public string GetMethodParamComment(XPathNavigator methodNode, string paramName)
        {
            XPathNavigator paramNode = methodNode.SelectSingleNode(string.Format(ParamXPath, paramName));
            return paramNode != null ? UtilsXmlCommentText.Humanize(paramNode.InnerXml) : string.Empty;
        }
    }
}
