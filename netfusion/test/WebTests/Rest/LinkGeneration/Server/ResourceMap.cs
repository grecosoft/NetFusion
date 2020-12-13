using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Server.Hal;

namespace WebTests.Rest.LinkGeneration.Server
{
    /// <summary>
    /// Resource map file used by the link-generation unit-tests.  Several links are defined
    /// for the StateModel class.  The unit tests request the resource by making an HTTP requests
    /// via the RequestClient and assert the links were properly added to the returned resource. 
    /// </summary>
    public class ResourceMap : HalResourceMap
    {
        protected override void OnBuildResourceMap()
        {
            Map<StateModel>()
                .LinkMeta<ResourceController>(meta => meta.Url("scenario-1", (c, r) => c.GetById(r.Id)))

                .LinkMeta<ResourceController>(meta => meta.Url("scenario-2", 
                        (c, r) => c.GetByIdAndRequiredParam(r.Id, r.Value2)))

                .LinkMeta<ResourceController>(meta => meta.Url("scenario-3", 
                        (c, r) => c.GetByIdWithOneOptionalParam(r.Id, r.Value3)))

                .LinkMeta<ResourceController>(meta => meta.Url("scenario-4",
                        (c, r) => c.GetByIdWithMultipleOptionalParams(r.Id, r.Value3, r.Value2)))

                .LinkMeta<ResourceController>(meta => meta.Url("scenario-5", (c, r) => c.Create(default)))

                .LinkMeta<ResourceController>(meta => meta.Url("scenario-6", (c, r) => c.Update(r.Id, default)))
                
                .LinkMeta(meta => meta.Href("scenario-20", HttpMethod.Options, "http://external/api/call/info"))

                .LinkMeta(meta => meta.Href("scenario-25", HttpMethod.Options,
                        r => $"http://external/api/call/{r.Id}/info/{r.Value2}"))

                .LinkMeta(meta => meta.Href("scenario-30", HttpMethod.Options, 
                        r => $"http://external/api/call/{r.Id}/info/{r.Value2}")
                            .SetTitle("test-title")
                            .SetType("test-type")
                            .SetHrefLang("test-href-lang"))

                .LinkMeta<ResourceController>(meta =>
                    meta.UrlTemplate<int, string, IActionResult>("scenario-31", c => c.AppendComment));

            Map<StateEmbeddedModel>()
                .LinkMeta(meta =>
                {
                    meta.Href("embedded-child", HttpMethod.Get, m => $"http://test/resource/{m.Id}");
                });
        }
    }
}
