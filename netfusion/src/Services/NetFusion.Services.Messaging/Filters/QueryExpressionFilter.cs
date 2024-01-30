using System.Collections;
using NetFusion.Common.Base.Entity;
using NetFusion.Common.Base.Scripting;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Filters;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Services.Messaging.Filters;

/// <summary>
/// A query filter registered as a post filter that delegates to the IEntityScriptingService 
/// to execute a script containing calculated properties that can be dynamically added to the
/// query result.  This applies to any result implementing the IAttributedEntity interface.
/// </summary>
public class QueryExpressionFilter(IEntityScriptingService scriptingService) : IPostMessageFilter
{
    private const string PropertyScriptName = "DynamicQueryReadModel";

    private readonly IEntityScriptingService _scriptingService = scriptingService ??
        throw new ArgumentNullException(nameof(scriptingService));

    public async Task OnPostFilterAsync(IMessage message)
    {
        if (message is not IQuery query)
        {
            return;
        }

        foreach (object resultItem in GetQueryResults(query).Where(IsAttributedResult))
        {
            await _scriptingService.ExecuteAsync(resultItem, PropertyScriptName).ConfigureAwait(false);
        }
    }

    private static bool IsAttributedResult(object resultItem)
    {
        return resultItem.GetType().CanAssignTo<IAttributedEntity>();
    }

    private static IEnumerable<object> GetQueryResults(IQuery query)
    {
        var messageResult = (IMessageWithResult)query;
        
        if (messageResult.MessageResult is IEnumerable items)
        {
            return items.Cast<object>();
        }
        return messageResult.MessageResult == null ? Enumerable.Empty<object>() : new[] { messageResult.MessageResult };
    }
}