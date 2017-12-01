using MongoDB.Driver;
using NetFusion.Domain.ReferenceTypes;
using NetFusion.Domain.ReferenceTypes.Core;
using NetFusion.MongoDB;
using System;
using System.Collections.Generic;

namespace NetFusion.Domain.MongoDB.ReferenceTypes
{
    public class ReferenceTypeRepository : IReferenceTypeRepository
    {
        private readonly IMongoDbClient<ReferenceTypeDb> _referenceTypeDb;
        private IMongoCollection<ReferenceType> ReferenceTypeColl { get; }

        public ReferenceTypeRepository(ReferenceTypeDb refTypeDb, IMongoDbClient<ReferenceTypeDb> referenceTypeDb)
        {
            // TODO:  expose the underlying db-settings off of the IMongoDbClient...
            // so it doesn't have to be injected separately and accessed off of the
            // already injected db-client.

            _referenceTypeDb = referenceTypeDb;
            this.ReferenceTypeColl = _referenceTypeDb.GetCollection<ReferenceType>(refTypeDb.CollectionName);
        }
         
        public void AddReferenceType(ReferenceType referenceType)
        {
            if (referenceType == null) throw new ArgumentNullException(nameof(referenceType));

            ReferenceTypeColl.InsertOne(referenceType);
        }

        public IEnumerable<ReferenceType> GetReferenceTypes()
        {
            return ReferenceTypeColl.Find(_ => true).ToList();
        }
    }
}
