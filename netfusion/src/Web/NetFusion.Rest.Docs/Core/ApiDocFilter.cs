using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NetFusion.Rest.Docs.Core
{
    public class ApiDocFilter : IAsyncResourceFilter
    {
        public Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            return Task.CompletedTask;
        }
    }
}