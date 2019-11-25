using NetFusion.Mapping;
using NetFusion.Messaging.Types;
using System;
using System.Collections;
using System.Threading.Tasks;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Messaging.Filters
{
    /// <summary>
    /// Simple mapping filter that determines if the consumer returned type matches the declared result type of 
    /// the query object.  If the types are not the same, the IObjectMapper instance is used to map the object 
    /// returned by the consumer to the type declared by the query.  This mapper also completes the mapping if 
    /// both the consumer returned object type and declared query result types are arrays.  Only arrays types 
    /// are checked since most often, the consumer will be returning results that should been materialized.
    /// </summary>
    public class ResultMapQueryFilter : IPostQueryFilter
    {
        private readonly IObjectMapper _objectMapper;

        public ResultMapQueryFilter(IObjectMapper objectMapper)
        {
            _objectMapper = objectMapper ?? throw new ArgumentNullException(nameof(objectMapper));
        }

        public Task OnPostExecute(IQuery query)
        {
            if (QueryResultCompatible(query))
            {
                // No mapping necessary.
                return Task.CompletedTask;
            }

            MapConsumerReturnObjectToQueryType(query);
            return Task.CompletedTask;
        }

        private static bool QueryResultCompatible(IQuery query)
        {
            // This is the case if the result type of the query is .NET dynamic
            // or the object type.
            if (query.DeclaredResultType == typeof(object))
            {
                return true;
            }

            // Check if the type of the returned result is compatible with
            // the return type of the query.
            return query.Result.GetType().CanAssignTo(query.DeclaredResultType);
        }

        private void MapConsumerReturnObjectToQueryType(IQuery query)
        {
            Type itemTargetType = GetTargetArrayType(query);

            // The query consumer didn't return an array of items so map single object.
            if (itemTargetType == null)
            {
                if (! _objectMapper.TryMap(query.Result, query.DeclaredResultType, out object mappedResult))
                {
                    throw new InvalidOperationException(
                        $"For query of type: {query.GetType()} the result of type: {query.Result.GetType()} " +
                         $" could not be mapped to the query declared result type of: {query.DeclaredResultType} or derived type.");
                }

                query.SetResult(mappedResult);
                return;
            }

            // The result is an array - map each item in the array.
            var results = (ICollection)query.Result;
            var mappedResults = Array.CreateInstance(itemTargetType, results.Count);

            int i = 0;
            foreach (object result in results)
            {
                if (! _objectMapper.TryMap(result, itemTargetType, out object mappedResult))
                {
                    throw new InvalidOperationException(
                        $"For query of type: {query.GetType()} the result type: {result.GetType()} " +
                        $"at element position: {i} could not be mapped to the query declared result type of: {itemTargetType} " +
                        "or derived type.");
                }
                
                mappedResults.SetValue(mappedResult, i++);
            }
            
            // Update the query results that will be returned.
            query.SetResult(mappedResults); 
        }

        private static Type GetTargetArrayType(IQuery query)
        {
            return query.DeclaredResultType.IsArray ? query.DeclaredResultType.GetElementType() : null;
        }
    }
}