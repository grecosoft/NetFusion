﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Common.Base.Exceptions;

/// <summary>
/// Base exception from which all other NetFusion specific exceptions derive.
/// This exception's Details property contains key/value pairs of information
/// associated with the exception.  When an inner exception or AggregateException
/// is specified, the details of any NetFusionException derived exceptions are
/// added to the dictionary.
/// </summary>
public class NetFusionException : Exception
{
    /// <summary>
    /// Value use to identity the context of the exception.  Used when asserting
    /// exceptions within unit-tests.
    /// </summary>
    public string? ExceptionId { get; }

    /// <summary>
    /// List of child exceptions associated with parent exception. 
    /// </summary>
    public IEnumerable<Exception> ChildExceptions { get; private set; } = Enumerable.Empty<Exception>();
        
    /// <summary>
    /// Dictionary of key/value pairs containing details of the exception. 
    /// </summary>
    public IDictionary<string, object> Details { get; } = new Dictionary<string, object>();
        
    protected NetFusionException() { }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">The message describing the exception.</param>
    /// <param name="exceptionId">Optional value used to identity the exception.</param>
    protected NetFusionException(string message, string? exceptionId = null)
        : base(message)
    {
        ExceptionId = exceptionId;
        Details["Message"] = message;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">The message describing the exception.</param>
    /// <param name="innerException">The source of the exception.  If an exception deriving 
    /// from NetFusionException, the details will be added as inner details to this exception.</param>
    /// <param name="exceptionId">Optional value used to identity the exception.</param>
    protected NetFusionException(string message, Exception? innerException, string? exceptionId = null)
        : base(message, innerException)
    {
        ExceptionId = exceptionId;
        Details["Message"] = message;

        if (innerException != null)
        {
            AddExceptionDetails(innerException);
        }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">The message describing the exception.</param>
    /// <param name="aggregateException">An aggregate exception associated with a task.</param>
    protected NetFusionException(string message, AggregateException? aggregateException)
        : this(message, aggregateException?.InnerException)
    {
        Details["Message"] = message;

        if (aggregateException != null)
        {
            SetChildExceptions(aggregateException.Flatten().InnerExceptions);
        }
    }
        
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">The message describing the exception.</param>
    /// <param name="detailKey">Value used to identify the exception details.</param>
    /// <param name="details">Object containing details of the application's state
    /// at the time of the exception.</param>
    /// <param name="exceptionId">Optional value used to identity the exception.</param>
    protected NetFusionException(string message, string detailKey, object details, 
        string? exceptionId = null)
        : this(message, exceptionId)
    {
        if (string.IsNullOrWhiteSpace(detailKey)) throw new ArgumentException(
            "Key to identify exception details not specified.", nameof(detailKey));

        Details[detailKey] = details ?? throw new ArgumentNullException(nameof(details),
            "Exception details cannot be null.");
    }
        
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">The message describing the exception.</param>
    /// <param name="innerException">The source of the exception.</param>
    /// <param name="detailKey">Value used to identify the exception details.</param>
    /// <param name="details">Object containing details of the application's state
    /// at the time of the exception.</param>
    /// <param name="exceptionId">Optional value used to identity the exception.</param>
    protected NetFusionException(string message, Exception innerException, 
        string detailKey, 
        object details, 
        string? exceptionId = null) : this(message, innerException, exceptionId)
    {
        if (string.IsNullOrWhiteSpace(detailKey)) throw new ArgumentException(
            "Key to identify exception details not specified.", nameof(detailKey));

        Details[detailKey] = details ?? throw new ArgumentNullException(nameof(details),
            "Exception details cannot be null.");
    }

    private void AddExceptionDetails(Exception innerException)
    {
        Details["InnerException"] = innerException.ToString();
            
        if (innerException is NetFusionException detailedEx)
        {
            Details["InnerDetails"] = detailedEx.Details;
        }
    }

    protected void SetChildExceptions(IEnumerable<Exception> exceptions)
    {
        ArgumentNullException.ThrowIfNull(exceptions);

        ChildExceptions = exceptions.ToArray();
            
        var detailedExceptions = ChildExceptions.OfType<NetFusionException>()
            .Select(de => de.Details)
            .ToArray();

        Details["InnerDetails"] = detailedExceptions;
    }
}