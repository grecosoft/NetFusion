using Microsoft.AspNetCore.Mvc;

namespace WebTests.Rest.ApiMetadata.Server
{
    public class QueryParamSource
    {
        [FromQuery]
        public string Filter { get; set; }
        
        [FromQuery]
        public string Version { get; set; }
    }
}