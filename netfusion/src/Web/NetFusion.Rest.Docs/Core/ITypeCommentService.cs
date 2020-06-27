using System;
using System.Xml.XPath;
using NetFusion.Rest.Docs.Models;

namespace NetFusion.Rest.Docs.Core
{
    public interface ITypeCommentService
    {
        XPathNavigator GetXmlCommentsForTypesAssembly(Type containedType);
        ApiResourceDoc GetResourceDoc(Type resourceType);
    }
}