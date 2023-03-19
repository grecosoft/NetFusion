using System;
using Microsoft.AspNetCore.Mvc;

namespace NetFusion.Web.UnitTests.Rest.ApiMetadata.Server;

public class HeaderParamSource
{
    [FromHeader]
    public string ClientId { get; set; }
        
    [FromHeader]
    public DateTime AsOfDate { get; set; }
}