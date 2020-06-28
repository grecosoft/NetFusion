using System;
using System.Reflection;
using System.Xml.XPath;
using NetFusion.Rest.Docs.Plugin;

namespace NetFusion.Rest.Docs.XmlDescriptions
{
    public class XmlCommentService : IXmlCommentService
    {
        private readonly IDocModule _docModule;

        public XmlCommentService(IDocModule docModule)
        {
            _docModule = docModule;
        }

        public XPathNavigator GetXmlCommentsForTypesAssembly(Type containedType)
        {
            Assembly typesAssembly = containedType.Assembly;
            return typesAssembly.GetXmlCommentDoc(_docModule.RestDocConfig.DescriptionDirectory);
        }
    }
}
