using NetFusion.Web.Rest.Server.Hal;

namespace NetFusion.Web.UnitTests.Rest.DocGeneration.Server;

public class DocResourceMap : HalResourceMap
{
    protected override void OnBuildResourceMap()
    {
        Map<ModelWithResourceLinks>()
            .LinkMeta<DocController>(
                meta => meta.Url("relation-1", (c, r) => c.GetResourceDetails(r.ModelId, r.VersionNumber)));

        Map<EmbeddedModelWithResourceLinks>()
            .LinkMeta<DocController>(
                meta => meta.Url("relation-2", (c, r) => c.GetEmbeddedResourceDetails(r.ModelId)));
    }
}