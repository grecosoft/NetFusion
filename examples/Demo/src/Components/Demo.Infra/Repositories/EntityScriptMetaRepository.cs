using Demo.Domain.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetFusion.Base.Scripting;

namespace Demo.Infra.Repositories
{
    public class EntityScriptMetaRepository : IEntityScriptMetaRepository
    {
        public Task<IEnumerable<EntityScript>> ReadAllAsync()
        {
            EntityScript[] entityScripts = StudentScripts.GetScripts();
            return Task.FromResult(entityScripts.AsEnumerable());
        }

        public Task<string> SaveAsync(EntityScript script)
        {
            throw new NotImplementedException();
        }
    }
}
