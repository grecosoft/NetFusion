using NetFusion.Rest.Server.Hal;

namespace InfrastructureTests.Web.Rest.LinkGeneration.Server
{
    public class ConventionBasedResourceMap  : HalResourceMap
    {
        public override void OnBuildResourceMap()
    {
  
            Map<LinkedResource>()
               .LinkMeta<ConventionBasedController>(meta => 
                {
                    meta.AutoMapSelfRelation();
                    meta.AutoMapUpdateRelations();
                });

            Map<LinkedResource2>()
               .LinkMeta<ConventionBasedController>(meta =>
               {
                   meta.AutoMapSelfRelation();
                   meta.AutoMapUpdateRelations();
               });

        }
    }
}
