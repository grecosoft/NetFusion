using System.Collections.Generic;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.XmlComments
{
    public class XmlResultComments : IResponseDescription
    {
        public IDictionary<string, object> Context { get; set; }
        
        public void Describe(ApiResponseDoc responseDoc, ApiResponseMeta responseMeta)
        {
            
        }
    }
}