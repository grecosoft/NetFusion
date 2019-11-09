using System;
using NetFusion.MongoDB;
using Demo.Domain.Entities;

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
