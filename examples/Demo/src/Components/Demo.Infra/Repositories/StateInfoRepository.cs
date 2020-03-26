using System.Linq;
using System.Threading.Tasks;
using Demo.Domain.Entities;
using MongoDB.Driver;
using NetFusion.MongoDB;
using Demo.Domain.Repositories;

namespace Demo.Infra.Repositories
{
    public class StateInfoRepository : IStateInfoRepository
    {
        private IMongoCollection<StateInfo> StateInfoColl { get; }

        public StateInfoRepository(IMongoDbClient<GeographicDb> geographicDb)
        {
            this.StateInfoColl = geographicDb.GetCollection<StateInfo>();
        }

        public async Task<string> Add(StateInfo stateInfo)
        {
            await StateInfoColl.InsertOneAsync(stateInfo);
            return stateInfo.Id;
        }

        public async Task<StateInfo> Read(string state)
        {
            var results = await StateInfoColl.Find(i => i.State == state)
                                             .ToListAsync();

            return results.FirstOrDefault();
        }
    }
}
