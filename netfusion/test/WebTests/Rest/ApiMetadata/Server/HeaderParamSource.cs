using System;
using Microsoft.AspNetCore.Mvc;

namespace WebTests.Rest.ApiMetadata.Server
{
    public class HeaderParamSource
    {
        [FromHeader]
        public string ClientId { get; set; }
        
        [FromHeader]
        public DateTime AsOfDate { get; set; }
    }
}