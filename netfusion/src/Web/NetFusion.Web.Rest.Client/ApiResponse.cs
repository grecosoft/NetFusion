using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using NetFusion.Web.Rest.Resources;

namespace NetFusion.Web.Rest.Client;

/// <summary>
/// Contains result properties for a submitted request with an optional untyped response state. 
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// The underlying message for which the result is associated.
    /// This underlying message was created from the ApiRequest.
    /// </summary>
    public HttpRequestMessage Request { get; }
        
    /// <summary>
    /// The underlying HTTP response message.
    /// </summary>
    public HttpResponseMessage Response { get; }

    /// <summary>
    /// Indicates if the response code is considered successful.
    /// </summary>
    public bool IsSuccessStatusCode { get; }

    /// <summary>
    /// The HTTP status code returned from the server.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// A descriptive phase usually related with the status code.
    /// </summary>
    public string ReasonPhase { get; }

    /// <summary>
    /// The response headers.
    /// </summary>
    public HttpResponseHeaders Headers { get;  }

    /// <summary>
    /// The media-type of the returned response content.  Null if the response didn't contain content.
    /// </summary>
    public string MediaType { get; }

    /// <summary>
    /// The character set of the returned response content.  Null if the response didn't contain content.
    /// </summary>
    public string CharSet { get; }

    /// <summary>
    /// The length of the returned response content.  Null if the response didn't contain content.
    /// </summary>
    public long? ContentLength { get; }

    /// <summary>
    /// Contains information about the server to which the request was submitted.
    /// </summary>
    public string Server { get; }

    /// <summary>
    /// Returns the value of the ETag header.
    /// </summary>
    public string ETag { get; }
        
    /// <summary>
    /// The deserialized returned content.
    /// </summary>
    public object State { get; protected set; }
        
    /// <summary>
    /// The returned content of the response as a string if an error status code was returned.
    /// </summary>
    public string ErrorContent { get; private set; }

    public ApiResponse(HttpResponseMessage responseMsg)
    {
        Response = responseMsg ?? throw new ArgumentNullException(nameof(responseMsg));
        Request = responseMsg.RequestMessage;
            
        IsSuccessStatusCode = responseMsg.IsSuccessStatusCode;
        StatusCode = responseMsg.StatusCode;
        ReasonPhase = responseMsg.ReasonPhrase;
        Headers = responseMsg.Headers;

        HttpContent httpContent = responseMsg.Content;

        ContentLength = httpContent.Headers.ContentLength;
        MediaType = httpContent.Headers.ContentType?.MediaType;
        CharSet = httpContent.Headers.ContentType?.CharSet;

        ETag = responseMsg.Headers.ETag?.Tag;
        Server = responseMsg.Headers.Server.ToString();
    }

    public ApiResponse(HttpResponseMessage responseMsg, object state)
        : this(responseMsg)
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
    }
        
    /// <summary>
    /// Sets the response context as a string for response error codes.
    /// </summary>
    /// <param name="value"></param>
    public void SetErrorContent(string value)
    {
        ErrorContent = value;
    }

    /// <summary>
    /// Allows the consumer handling the response to throw an HttpRequestException
    /// if a success HTTP status code was not received.
    /// </summary>
    public void ThrowIfNotSuccessStatusCode()
    {
        if (IsSuccessStatusCode)
        {
            return;
        }

        string errMessage = $"Response status code does not indicate success: ({StatusCode})";
        if (ErrorContent != null)
        {
            var serverEx = new Exception(ErrorContent);
            throw new HttpRequestException(errMessage, serverEx);
        }
            
        throw new HttpRequestException(errMessage);
    }
}

/// <summary>
/// Derived ApiResponse with the body deserialized into typed content.
/// </summary>
/// <typeparam name="TContent">The type of the response content.</typeparam>
public class ApiResponse<TContent> : ApiResponse
    where TContent : class
{
    public TContent Content { get; }    

    public ApiResponse(HttpResponseMessage responseMsg, TContent content) 
        : base(responseMsg, content)
    {
        Content = content;
    }
}

/// <summary>
/// Derived ApiResponse that supporting HAL based resource responses.
/// </summary>
public class ApiHalResponse<TModel> : ApiResponse
    where TModel : class
{
    public HalResource<TModel> Resource { get; }

    public ApiHalResponse(HttpResponseMessage responseMsg, HalResource<TModel> resource)
        : base(responseMsg, resource)
    {
        Resource = resource;
    }        
}