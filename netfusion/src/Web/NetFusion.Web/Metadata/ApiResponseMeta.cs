using System;

namespace NetFusion.Web.Metadata;

/// <summary>
/// Information about a WebApi method response.
/// </summary>
public class ApiResponseMeta
{   
    /// <summary>
    /// The HTTP status code associated with the response.
    /// </summary>
    public int Status { get; }
        
    /// <summary>
    /// The optional model type associated with the HTTP status code.
    /// </summary>
    public Type? ModelType { get; set; }
        
    public ApiResponseMeta(int status, Type modelType)
    {
        if (modelType == null) throw new ArgumentNullException(nameof(modelType));
            
        Status = status;
        ModelType = modelType == typeof(void) ? null : modelType;
    }
}