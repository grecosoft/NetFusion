using System.Net;
using System.Xml.Linq;

namespace NetFusion.Rest.Server.Documentation.Core
{
    /// <summary>
    /// Documentation for the possible HTTP status codes returned by
    /// a controller's action method.
    /// </summary>
    public class DocHttpCode
    {
        public DocHttpCode(XElement element)
        {
            Code = element.Attribute("httpCode")?.Value;
            Description = element.Attribute("description")?.Value;
        }

        public DocHttpCode(HttpStatusCode httpCode)
        {
            Code = httpCode.ToString();
        }

        public string Code { get; }
        public string Description { get; set; }
    }
}
