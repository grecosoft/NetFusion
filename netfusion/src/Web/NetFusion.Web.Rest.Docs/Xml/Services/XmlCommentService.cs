﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Xml.XPath;
using NetFusion.Web.Rest.Docs.Plugin;
using NetFusion.Web.Rest.Docs.Xml.Extensions;

namespace NetFusion.Web.Rest.Docs.Xml.Services;

/// <summary>
/// Service used to read documentation for types and members from XML
/// generated code files.
/// </summary>
public class XmlCommentService(IDocModule docModule) : IXmlCommentService
{
    private const string MemberXPath = "/doc/members/member[@name='{0}']";
    private const string ParamXPath = "param[@name='{0}']";

    private readonly IDocModule _docModule = docModule ?? throw new ArgumentNullException(nameof(docModule));
    private readonly ConcurrentDictionary<Assembly, XPathNavigator> _xmlAssemblyComments = new();

    public XPathNavigator GetXmlCommentsForTypesAssembly(Type containedType)
    {
        ArgumentNullException.ThrowIfNull(containedType);

        Assembly typesAssembly = containedType.Assembly;

        return _xmlAssemblyComments.GetOrAdd(typesAssembly, _ =>
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
        ArgumentNullException.ThrowIfNull(classType);

        XPathNavigator xmlCommentsDoc = GetXmlCommentsForTypesAssembly(classType);

        string typeMemberName = UtilsXmlComment.GetMemberNameForType(classType);
        return xmlCommentsDoc?.SelectSingleNode(string.Format(MemberXPath, typeMemberName));
    }

    public string GetTypeComments(Type classType)
    {
        ArgumentNullException.ThrowIfNull(classType);

        XPathNavigator memberNode = GetTypeNode(classType);

        var summaryNode = memberNode?.SelectSingleNode("summary");
        return summaryNode != null ? UtilsXmlCommentText.Humanize(summaryNode.InnerXml) : string.Empty;
    }

    public XPathNavigator GetMethodNode(MethodInfo methodInfo)
    {
        ArgumentNullException.ThrowIfNull(methodInfo);

        XPathNavigator xmlCommentsDoc = GetXmlCommentsForTypesAssembly(methodInfo.DeclaringType);

        string methodMemberName = UtilsXmlComment.GetMemberNameForMethod(methodInfo);
        return xmlCommentsDoc?.SelectSingleNode(string.Format(MemberXPath, methodMemberName));
    }

    public string GetMethodComments(MethodInfo methodInfo)
    {
        ArgumentNullException.ThrowIfNull(methodInfo);

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
        ArgumentNullException.ThrowIfNull(methodNode);

        if (string.IsNullOrWhiteSpace(paramName))
            throw new ArgumentException("Parameter name must be specified.", nameof(paramName));
            
        XPathNavigator paramNode = methodNode.SelectSingleNode(string.Format(ParamXPath, paramName));
        return paramNode != null ? UtilsXmlCommentText.Humanize(paramNode.InnerXml) : string.Empty;
    }
}