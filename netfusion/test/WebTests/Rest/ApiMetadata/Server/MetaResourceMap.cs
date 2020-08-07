using NetFusion.Rest.Server.Hal;

namespace WebTests.Rest.ApiMetadata.Server
{
    public class MetaResourceMap : HalResourceMap
    {
        protected override void OnBuildResourceMap()
        {
            Map<MetaResource>()
                .LinkMeta<MetadataController>(meta =>
                {
                    
                });
        }
    }
}