using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Server.Hal;

namespace WebTests.Rest.LinkGeneration.Server
{
    /// <summary>
    /// Resource map file used by the link-generation unit-tests.
    /// Several links are defined for the LinkedResource test resource. 
    /// The unit tests request the resource by making an HTTP requests
    /// via the RequestClient and assert the links were properly added
    /// to the returned resource. 
    /// </summary>
    public class LinkedResourceMap : HalResourceMap
    {
        protected override void OnBuildResourceMap()
        {
            // Test link definition specifying link using typed-safe expression selecting
            // a controller's action method.  The generated link will be the URL template
            // for the corresponding controller method with route parameters populated from
            // the corresponding resource properties.
            Map<LinkedResource>()
                .LinkMeta<LinkedResourceController>(
                    meta => meta.Url("scenario-1", (c, r) => c.GetById(r.Id)))

                .LinkMeta<LinkedResourceController>(
                    meta => meta.Url("scenario-2", (c, r) => c.GetByIdAndRequiredParam(r.Id, r.Value2)))

                .LinkMeta<LinkedResourceController>(
                    meta => meta.Url("scenario-3", (c, r) => c.GetByIdWithOneOptionalParam(r.Id, r.Value3)))

                .LinkMeta<LinkedResourceController>(
                    meta => meta.Url("scenario-4",
                        (c, r) => c.GetByIdWithMultipleOptionalParams(r.Id, r.Value3, r.Value2)))

                .LinkMeta<LinkedResourceController>(
                    meta => meta.Url("scenario-5",
                        (c, r) => c.Create(default(WebTests.Rest.LinkGeneration.Server.LinkedResource))))

                .LinkMeta<LinkedResourceController>(
                    meta => meta.Url("scenario-6",
                        (c, r) => c.Update(r.Id, default(WebTests.Rest.LinkGeneration.Server.LinkedResource))))
                
                .LinkMeta<LinkedResourceController>(
                    meta => meta.Href("scenario-20", HttpMethod.Options, "http://external/api/call/info"))

                .LinkMeta<LinkedResourceController>(
                    meta => meta.Href("scenario-25", HttpMethod.Options,
                        r => $"http://external/api/call/{r.Id}/info/{r.Value2}"))

                .LinkMeta<LinkedResourceController>(
                    meta => meta.Href("scenario-30", HttpMethod.Options,
                            r => $"http://external/api/call/{r.Id}/info/{r.Value2}")
                        .SetName("test-name")
                        .SetTitle("test-title")
                        .SetType("test-type")
                        .SetHrefLang("test-href-lang"))

                .LinkMeta<LinkedResourceController>(meta =>
                    meta.UrlTemplate<int, string, IActionResult>("scenario-31", c => c.AppendComment));
        }
    }
}
