using Demo.Domain.Entities;
using NetFusion.MongoDB;

namespace Demo.Infra.Repositories
{
    public class StateInfoMap : EntityClassMap<StateInfo>
    {
        public StateInfoMap()
        {
            CollectionName = "info.states";
            AutoMap();
            MapStringPropertyToObjectId(m => m.Id);
        }
    }
}
