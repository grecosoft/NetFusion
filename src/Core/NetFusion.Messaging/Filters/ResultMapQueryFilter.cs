using NetFusion.Mapping;
using NetFusion.Messaging.Types;
using System;
using System.Collections;
using System.Threading.Tasks;

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
            if (QueryResultMatchesConsumerReturnType(query))
            {
                return Task.CompletedTask;
            }

            MapConsumerReturnObjectToQueryType(query);
            return Task.CompletedTask;
        }

        private static bool QueryResultMatchesConsumerReturnType(IQuery query)
        {
            // This is the case if the result type is .NET dynamic.
            if (query.DeclaredResultType == typeof(object))
            {
                return true;
            }

            return query.Result.GetType() == query.DeclaredResultType;
        }

        private void MapConsumerReturnObjectToQueryType(IQuery query)
        {
            Type itemTargetType = GetTargetArrayType(query);

            // The query consumer didn't return an array of items so map single object.
            if (itemTargetType == null)
            {
                object mappedResult = _objectMapper.Map(query.Result, query.DeclaredResultType);
                query.SetResult(mappedResult);
                return;
            }

            // The result is an array - map each item in the array.
            var results = (ICollection)query.Result;
            var mappedResults = Array.CreateInstance(itemTargetType, results.Count);

            int i = 0;
            foreach (object result in results)
            {
                object mappedResult = _objectMapper.Map(result, itemTargetType);
                mappedResults.SetValue(mappedResult, i++);
            }
            
            // Update the query results that will be returned.
            query.SetResult(mappedResults); 
        }

        private static Type GetTargetArrayType(IQuery query)
        {                   
            if (! query.DeclaredResultType.IsArray)
            {
                return null;
            }

            return query.DeclaredResultType.GetElementType();           
        }
    }
}