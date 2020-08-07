using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Xml.XPath;
using NetFusion.Rest.Docs.Plugin;
using NetFusion.Rest.Docs.Xml.Extensions;

namespace NetFusion.Rest.Docs.Xml.Services
{
    /// <summary>
    /// Service used to read documentation for types and members from XML
    /// generated code files.
    /// </summary>
    public class XmlCommentService : IXmlCommentService
    {
        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        private const string ParamXPath = "param[@name='{0}']";

        private readonly IDocModule _docModule;
        private readonly ConcurrentDictionary<Assembly, XPathNavigator> _xmlAssemblyComments;

        public XmlCommentService(IDocModule docModule)
        {
            _docModule = docModule ?? throw new ArgumentNullException(nameof(docModule));
            _xmlAssemblyComments = new ConcurrentDictionary<Assembly, XPathNavigator>();
        }

        public XPathNavigator GetXmlCommentsForTypesAssembly(Type containedType)
        {
            if (containedType == null) throw new ArgumentNullException(nameof(containedType));
            
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
            if (classType == null) throw new ArgumentNullException(nameof(classType));
            
            XPathNavigator xmlCommentsDoc = GetXmlCommentsForTypesAssembly(classType);

            string typeMemberName = UtilsXmlComment.GetMemberNameForType(classType);
            return xmlCommentsDoc?.SelectSingleNode(string.Format(MemberXPath, typeMemberName));
        }

        public string GetTypeComments(Type classType)
        {
            if (classType == null) throw new ArgumentNullException(nameof(classType));
            
            XPathNavigator memberNode = GetTypeNode(classType);

            var summaryNode = memberNode?.SelectSingleNode("summary");
            return summaryNode != null ? UtilsXmlCommentText.Humanize(summaryNode.InnerXml) : string.Empty;
        }

        public XPathNavigator GetMethodNode(MethodInfo methodInfo)
        {
            if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

            XPathNavigator xmlCommentsDoc = GetXmlCommentsForTypesAssembly(methodInfo.DeclaringType);

            string methodMemberName = UtilsXmlComment.GetMemberNameForMethod(methodInfo);
            return xmlCommentsDoc?.SelectSingleNode(string.Format(MemberXPath, methodMemberName));
        }

        public string GetMethodComments(MethodInfo methodInfo)
        {
            if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));
            
            XPathNavigator memberNode = GetMethodNode(methodInfo);
            if (memberNode == null)
            {
                return string.Empty;
            }

            var summaryInnerXml = memberNode.SelectSingleNode("summary")?.InnerXml;
            var returnsInnerXml = memberNode.SelectSingleNode("returns")?.InnerXml;

            return (UtilsXmlCommentText.Humanize(summaryInnerXml ?? string.Empty)
                + " " + UtilsXmlCommentText.Humanize(returnsInnerXml ?? string.Empty)).Trim();
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
            if (methodNode == null) throw new ArgumentNullException(nameof(methodNode));
            
            if (string.IsNullOrWhiteSpace(paramName))
                throw new ArgumentException("Parameter name must be specified.", nameof(paramName));
            
            XPathNavigator paramNode = methodNode.SelectSingleNode(string.Format(ParamXPath, paramName));
            return paramNode != null ? UtilsXmlCommentText.Humanize(paramNode.InnerXml) : string.Empty;
        }
    }
}
