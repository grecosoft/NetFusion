using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Domain.ReferenceTypes.Core
{
    public class NullReferenceTypeRepository : IReferenceTypeRepository
    {
        public void AddReferenceType(ReferenceType referenceType)
        {
          
        }

        public IEnumerable<ReferenceType> GetReferenceTypes()
        {
            return Enumerable.Empty<ReferenceType>();
        }
    }
}
