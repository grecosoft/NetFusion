using System;
using System.Xml.XPath;

namespace NetFusion.Rest.Docs.XmlDescriptions
{
    public interface IXmlCommentService
    {
        XPathNavigator GetXmlCommentsForTypesAssembly(Type containedType);
    }
}
