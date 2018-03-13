using NetFusion.Base.Entity;
using NetFusion.Base.Scripting;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Types;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetFusion.Messaging.Filters
{
    /// <summary>
    /// A query filter registered as a post filter that delegates to the IEntityScriptingService 
    /// to execute a script containing calculated properties that can be dynamically added to the
    /// query result.  This applies to any result implementing the IAttributedEntity interface.
    /// </summary>
    public class ExpressionQueryFilter : IPostQueryFilter
    {
        private const string PropertyScriptName = "AttributedReadModel";

        private readonly IEntityScriptingService _scriptingService;

        public ExpressionQueryFilter(IEntityScriptingService scriptingService)
        {
            _scriptingService = scriptingService;
        }

        public async Task OnPostExecute(IQuery query)
        {
            foreach (object resultItem in GetQueryResults(query).Where(IsAttributedResult))
            {
                await _scriptingService.ExecuteAsync(resultItem, PropertyScriptName);
            }
        }

        private bool IsAttributedResult(object resultItem)
        {
            return resultItem.GetType().IsDerivedFrom<IAttributedEntity>();
        }

        private IEnumerable<object> GetQueryResults(IQuery query)
        {
            if (query.Result is IEnumerable items)
            {
                return items.Cast<object>();
            }
            return new[] { query.Result };
        }
    }
}
