using NetFusion.Common.Base.Exceptions;

namespace NetFusion.Services.Serialization;

public class SerializationException(string message, string? exceptionId = null)
    : NetFusionException(message, exceptionId);