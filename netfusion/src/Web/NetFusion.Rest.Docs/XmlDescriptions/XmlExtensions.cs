using System.IO;
using System.Reflection;
using System.Xml.XPath;

namespace NetFusion.Rest.Docs.XmlDescriptions
{
    public static class XmlExtensions
    {
        public static XPathNavigator GetXmlCommentDoc(this Assembly assembly, string basePath)
        {
            string fileName = Path.Join(basePath, $"{assembly.GetName().Name}.xml");
            return File.Exists(fileName) ? new XPathDocument(fileName).CreateNavigator() : null;
        }
    }
}