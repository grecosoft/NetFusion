using Microsoft.AspNetCore.Mvc;

namespace NetFusion.Web.UnitTests.Rest.ApiMetadata.Server;

public class QueryParamSource
{
    [FromQuery]
    public string Filter { get; set; }
        
    [FromQuery]
    public string Version { get; set; }
}