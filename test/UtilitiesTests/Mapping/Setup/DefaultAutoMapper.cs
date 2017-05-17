using Mapster;
using NetFusion.Utilities.Core;

namespace UtilitiesTests.Mapping.Setup
{
    // For the unit-tests, Mapster will be  used for auto mapping.  The host application can register a type
    // implementing the IAutoMapper interface that the MappingModule will create a single instance of used by 
    // the ObjectMapper to automatically map object properties if an explicit mapping strategy is not registered.
    public class DefaultAutoMapper : IAutoMapper
    {
        public TTarget Map<TTarget>(object obj) where TTarget : class
        {
            return obj.Adapt<TTarget>();
        }
    }
}
