using System.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Filters;
using NetFusion.Messaging.Types.Contracts;
using NetFusion.Services.Mapping;

namespace NetFusion.Services.Messaging.Filters;

/// <summary>
/// Simple mapping filter that determines if the consumer returned type matches the declared result type of 
/// the query object.  If the types are not the same, the IObjectMapper instance is used to map the object 
/// returned by the consumer to the type declared by the query.  This mapper also completes the mapping if 
/// both the consumer returned object type and declared query result types are arrays.  Only arrays types 
/// are checked since most often, the consumer will be returning results that should been materialized.
/// </summary>
public class QueryResultMapFilter : IPostMessageFilter
{
    private readonly IObjectMapper _objectMapper;

    public QueryResultMapFilter(IObjectMapper objectMapper)
    {
        _objectMapper = objectMapper ?? throw new ArgumentNullException(nameof(objectMapper));
    }

    public Task OnPostFilterAsync(IMessage message)
    {
        if (message is not IQuery query)
        {
            return Task.CompletedTask;
        }
        
        var messageResult = (IMessageWithResult)query;
            
        if (QueryResultCompatible(messageResult))
        {
            // No mapping necessary.
            return Task.CompletedTask;
        }

        MapConsumerReturnObjectToQueryType(messageResult);
        return Task.CompletedTask;
    }

    private static bool QueryResultCompatible(IMessageWithResult messageResult)
    {
        // This is the case if the result type of the query is .NET dynamic
        // or the object type.
        if (messageResult.MessageResult == null || messageResult.DeclaredResultType == typeof(object))
        {
            return true;
        }

        // Check if the type of the returned result is compatible with
        // the result type of the query.
        return messageResult.MessageResult.GetType().CanAssignTo(messageResult.DeclaredResultType);
    }

    private void MapConsumerReturnObjectToQueryType(IMessageWithResult messageResult)
    {
        if (messageResult.MessageResult == null)
        {
            return;
        }
            
        Type? itemTargetType = GetTargetArrayType(messageResult);

        // The query consumer didn't return an array of items so map single object.
        if (itemTargetType == null)
        {
            if (! _objectMapper.TryMap(messageResult.MessageResult, messageResult.DeclaredResultType, out object? mappedResult))
            {
                throw new InvalidOperationException(
                    $"For query of type: {messageResult.GetType()} the result of type: {messageResult.MessageResult.GetType()} could not " +
                    $"be mapped to the query declared result type of: {messageResult.DeclaredResultType} or derived type.");
            }

            // Update the query results that will be returned.
            messageResult.SetResult(mappedResult);
            return;
        }

        // The result is an array - map each item in the array.
        var results = (ICollection)messageResult.MessageResult;
        var mappedResults = Array.CreateInstance(itemTargetType, results.Count);

        int i = 0;
        foreach (object result in results)
        {
            if (! _objectMapper.TryMap(result, itemTargetType, out object? mappedResult))
            {
                throw new InvalidOperationException(
                    $"For query of type: {messageResult.GetType()} the result type: {result.GetType()} at element " +
                    $"position: {i} could not be mapped to the query declared result type of: {itemTargetType} " +
                    "or derived type.");
            }
                
            mappedResults.SetValue(mappedResult, i++);
        }
            
        // Update the query results that will be returned.
        messageResult.SetResult(mappedResults); 
    }

    private static Type? GetTargetArrayType(IMessageWithResult query)
    {
        return query.DeclaredResultType.IsArray ? query.DeclaredResultType.GetElementType() : null;
    }
}