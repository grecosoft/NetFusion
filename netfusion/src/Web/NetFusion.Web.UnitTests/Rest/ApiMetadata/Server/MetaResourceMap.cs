using NetFusion.Web.Rest.Server.Hal;

namespace NetFusion.Web.UnitTests.Rest.ApiMetadata.Server;

public class MetaResourceMap : HalResourceMap
{
    protected override void OnBuildResourceMap()
    {
        Map<MetaResource>()
            .LinkMeta<MetadataController>(_ =>
            {
                    
            });
    }
}