using NetFusion.Common.Base.Exceptions;

namespace NetFusion.Services.Mapping;

public class MappingException : NetFusionException
{
    public MappingException(string message, string? exceptionId = null) :
        base(message, exceptionId)
    {
        
    }
}