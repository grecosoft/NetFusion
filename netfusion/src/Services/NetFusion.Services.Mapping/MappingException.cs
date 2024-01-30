using NetFusion.Common.Base.Exceptions;

namespace NetFusion.Services.Mapping;

public class MappingException(string message, string? exceptionId = null) : NetFusionException(message, exceptionId);