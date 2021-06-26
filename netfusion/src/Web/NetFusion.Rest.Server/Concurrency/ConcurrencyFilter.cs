using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using NetFusion.Base.Entity;
using NetFusion.Rest.Resources;

namespace NetFusion.Rest.Server.Concurrency
{
    /// <summary>
    /// Global action filter that checks if the request has the If-Match header defined, and if present,
    /// sets the ClientToken on the ConcurrencyContext instance associated with the current request.
    /// After the Api method is called, checks if the CurrentToken was recorded and sets as the ETag
    /// header on the response.  
    /// </summary>
    public class ConcurrencyFilter : IAsyncActionFilter
    {
        private readonly EntityContext _concurrencyContext;
        
        public ConcurrencyFilter(EntityContext concurrencyContext)
        {
            _concurrencyContext = concurrencyContext ?? throw new ArgumentNullException(nameof(concurrencyContext));
        }
    
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            SetClientToken(context);
            
            var executedContext = await next();
            
            SetEtagHeader(executedContext);
        }

        private void SetClientToken(ActionExecutingContext context)
        {
            if (! context.HttpContext.Request.Headers.TryGetValue(HeaderNames.IfMatch, out var headerValues))
            {
                return;
            }
            
            string isMatchHeader = headerValues.FirstOrDefault();
            if (isMatchHeader != null)
            {
                _concurrencyContext.SetClientToken(isMatchHeader);
            }
        }

        private void SetEtagHeader(ActionExecutedContext context)
        {
            // If explicitly set in code, the CurrentToken will most often correspond to the entity's 
            // current token value after a failed attempt was made to update the entity.  
            if (! string.IsNullOrWhiteSpace(_concurrencyContext.CurrentToken))
            {
                context.HttpContext.Response.Headers[HeaderNames.ETag] =
                    new StringValues(_concurrencyContext.CurrentToken);
                
                return;
            }
            
            // If the CurrentToken was not explicitly set, then this is the initial Api response
            // returning the resource to the client and a check is made to determine if the returned
            // resource supports optimistic-concurrency.
            SetEtagHeaderFromResponse(context);
        }

        private static void SetEtagHeaderFromResponse(ActionExecutedContext context)
        {
            if (context.Result is not ObjectResult objResult) return;

            object result = objResult.Value is HalResource resource ? resource.ModelValue : objResult.Value;

            if (result is IEntityToken concurrencyToken && concurrencyToken.Token != null)
            {
                context.HttpContext.Response.Headers[HeaderNames.ETag] =
                    new StringValues(concurrencyToken.Token);
            }
        }
    }
}