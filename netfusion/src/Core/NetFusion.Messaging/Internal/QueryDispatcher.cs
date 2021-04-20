using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Logging;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Tasks;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Filters;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Internal
{
    /// <summary>
    /// Responsible for dispatching a query to its consumer and invoking
    /// any registered query filters.
    /// </summary>
    public class QueryDispatcher 
    {
        private readonly ILogger<QueryDispatcher> _logger;
        private readonly IServiceProvider _services;
        private readonly IEnumerable<IQueryFilter> _queryFilters;
        
        // Dependent modules.
        private readonly IQueryDispatchModule _dispatchModule;

        public QueryDispatcher(
            ILogger<QueryDispatcher> logger,
            IServiceProvider services,
            IQueryDispatchModule dispatchModule,
            IEnumerable<IQueryFilter> queryFilters)
        {
            _logger = logger;
            _services = services;
            _dispatchModule = dispatchModule;
            
            // The order in which the filters are called should not matter.
            // However,they are applied in the order configured.
            _queryFilters = queryFilters.OrderByMatchingType(_dispatchModule.DispatchConfig.QueryFilters);
        }

        public async Task<TResult> Dispatch<TResult>(IQuery<TResult> query, 
            CancellationToken cancellationToken = default)
        {
            if (query == null) throw new ArgumentNullException(nameof(query),
                "Query to dispatch can't be null.");

            QueryDispatchInfo dispatchInfo = _dispatchModule.GetQueryDispatchInfo(query.GetType());

            try
            {
                await InvokeDispatcher(dispatchInfo, query, cancellationToken).ConfigureAwait(false);
            }
            catch (QueryDispatchException ex)
            {
                // Log the details of the dispatch exception and rethrow.
                _logger.LogError(ex, "Exception dispatching query.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Exception dispatching query.");
                throw new QueryDispatchException("Exception dispatching query.", ex);
            }

            return query.Result;
        }
        
        // ----------------------------- [Query Dispatching] -----------------------------

        // Creates an instance of the consumer that will execute the query and calls
        // it between the pre and post filters.
        private async Task InvokeDispatcher(QueryDispatchInfo dispatcher, IQuery query, CancellationToken cancellationToken)
        {
            LogQueryDispatch(dispatcher, query);
            
            await ApplyFilters<IPreQueryFilter>(query, _queryFilters, (f, q) => f.OnPreExecuteAsync(q));
            await InvokeQueryHandler(dispatcher, query, cancellationToken);
            await ApplyFilters<IPostQueryFilter>(query, _queryFilters, (f, q) => f.OnPostExecuteAsync(q));
        }
        
        // Executes a list of asynchronous filters and awaits their completion.  Once completed,
        // any task error(s) are checked and raised.  The passed list of query filters are filtered
        // by the specified filter type of TFilter (pre/post).
        private async Task ApplyFilters<TFilter>(IQuery query, IEnumerable<IQueryFilter> filters, 
            Func<TFilter, IQuery, Task> executeFilter) where TFilter : class, IQueryFilter
        {
            var filtersByType = filters.OfType<TFilter>().ToArray();

            LogQueryFilters<TFilter>(filtersByType);
            
            TaskListItem<TFilter>[] taskList = null;
            try
            {
                taskList = filtersByType.Invoke(query, executeFilter);
                await taskList.WhenAll();
            }
            catch (Exception ex)
            {
                if (taskList != null)
                {
                    var filterErrors = taskList.GetExceptions(GetFilterException);
                    if (filterErrors.Any())
                    {
                        throw new QueryDispatchException("Exception when invoking query filters.", filterErrors);
                    }
                }

                throw new QueryDispatchException("Exception when invoking query filters.", ex);
            }
        }

        private async Task InvokeQueryHandler(QueryDispatchInfo dispatcher, IQuery query,
            CancellationToken cancellationToken)
        {
            try
            {
                var consumer = (IQueryConsumer)_services.GetRequiredService(dispatcher.ConsumerType);
                await dispatcher.Dispatch(query, consumer, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new QueryDispatchException("Exception Dispatching Query.", dispatcher, ex);
            }
        }
        
        // ----------------------------- [Logging] -----------------------------
        
        private void LogQueryDispatch(QueryDispatchInfo dispatchInfo, IQuery query)
        {
            var handlerInfo = new {
                Consumer = dispatchInfo.ConsumerType,
                Handler = dispatchInfo.HandlerMethod.Name
            };
            
            var log = LogMessage.For(LogLevel.Debug, "Query {QueryType} Dispatched", query.GetType())
                .WithProperties(
                    new LogProperty { Name = "Query", Value = query }, 
                    new LogProperty { Name = "Handler", Value = handlerInfo });

            _logger.Log(log);
        }

        private void LogQueryFilters<TFilter>(IEnumerable<IQueryFilter> filters) 
            where TFilter : IQueryFilter
        {
            var log = LogMessage.For(LogLevel.Debug, "Applying {FilterType} Query Filters", typeof(TFilter).Name)
                .WithProperties(new LogProperty
                {
                    Name = "FilterTypes", 
                    Value = filters.Select(f => f.GetType().FullName).ToArray()
                });
            
            _logger.Log(log);
        }
        
        // ------------------------------- [Exceptions] --------------------------
        private static QueryFilterException GetFilterException<T>(TaskListItem<T> taskItem)
            where T : class, IQueryFilter
        {
            return new("Exception Applying Query Filter", taskItem.Invoker, taskItem.Task.Exception);
        }
    }
}
