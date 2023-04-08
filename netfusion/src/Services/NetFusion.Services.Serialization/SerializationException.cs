using NetFusion.Common.Base.Exceptions;

namespace NetFusion.Services.Serialization;

public class SerializationException : NetFusionException
{
    public SerializationException(string message, string? exceptionId = null)
        : base(message, exceptionId)
    {
      
    }
}