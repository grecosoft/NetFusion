using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using NetFusion.Common.Base.Entity;
using NetFusion.Common.Base.Scripting;
using NetFusion.Services.Roslyn.Internal;

namespace NetFusion.Services.UnitTests.Roslyn;

public static class EntityEvaluationTestExtensions
{
    public static IList<EntityExpression> AddExpression(this IList<EntityExpression> expressions,
        string propertyName,
        string expression)
    {
        expressions.Add(new EntityExpression(expression, 0, propertyName));
        return expressions;
    }

    public static IList<EntityExpression> AddExpression(this IList<EntityExpression> expressions,
        string expression)
    {
        expressions.Add(new EntityExpression(expression, 0));
        return expressions;
    }

    public static IEntityScriptingService CreateService<T>(this IList<EntityExpression> expressions)
        where T : IAttributedEntity
    {
        var es = new EntityScript(
            Guid.NewGuid().ToString(),
            "default", 
            typeof(T).AssemblyQualifiedName!,
            new ReadOnlyCollection<EntityExpression>(expressions));

        es.ImportedAssemblies = new[] { typeof(Common.Extensions.ObjectExtensions).Assembly.FullName! };
        es.ImportedNamespaces = new[] { typeof(Common.Extensions.ObjectExtensions).Namespace! };

        var loggerFactory = new LoggerFactory();
                
        var evalSrv = new EntityScriptingService(loggerFactory.CreateLogger<EntityScriptingService>());
        evalSrv.Load(new [] { es });
        return evalSrv;
    }


}