using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Extensions.Logging;
using NetFusion.Common.Base.Entity;
using NetFusion.Common.Base.Scripting;
using NetFusion.Services.Roslyn.Internal;

// ReSharper disable UseObjectOrCollectionInitializer

namespace NetFusion.Services.Roslyn.Testing;

/// <summary>
/// Provides extension methods used to test expression evaluations.
/// </summary>
public static class EvaluationExtensions
{
    /// <summary>
    /// Adds an expression, when evaluated, will dynamically add a property
    /// to the entity having the result of the evaluated expression.
    /// </summary>
    /// <param name="expressions">The list of expressions.</param>
    /// <param name="propertyName">The name of the property that will be dynamically
    /// added to the entity with a value equal to the evaluated expression.</param>
    /// <param name="expression">The expression to be evaluated. </param>
    /// <returns>The list of expressions with added expression.</returns>
    public static IList<EntityExpression> AddExpression(this IList<EntityExpression> expressions,
        string propertyName,
        string expression) 
    {
        if (expressions == null) throw new ArgumentNullException(nameof(expressions));
            
        expressions.Add(new EntityExpression(expression, expressions.Count, propertyName));
        return expressions;
    }

    /// <summary>
    /// Adds an expression that updates an existing property on the entity.
    /// </summary>
    /// <param name="expressions">The list of expressions.</param>
    /// <param name="expression">The expression to be evaluated.</param>
    /// <returns>The list of expressions with added expression.</returns>
    public static IList<EntityExpression> AddExpression(this IList<EntityExpression> expressions,
        string expression) 
    {
        if (expressions == null) throw new ArgumentNullException(nameof(expressions));
            
        expressions.Add(new EntityExpression(expression, expressions.Count));
        return expressions;
    }

    /// <summary>
    /// Creates and initializes a IEntityScriptingService from a list of expressions.
    /// </summary>
    /// <param name="expressions">Expressions to evaluated.</param>
    /// <param name="initialAttributes">The initial values to be use for dynamic properties
    /// if not already present on the entity.</param>
    /// <typeparam name="TEntity">The entity type associated with the list of expressions.</typeparam>
    /// <returns>The created service.</returns>
    public static IEntityScriptingService CreateService<TEntity>(this IList<EntityExpression> expressions,
        IDictionary<string, object>? initialAttributes = null)
        where TEntity : IAttributedEntity
    {
        if (expressions == null) throw new ArgumentNullException(nameof(expressions));
            
        var es = new EntityScript(
            Guid.NewGuid().ToString(),
            "default",
            typeof(TEntity).AssemblyQualifiedName!,
            new ReadOnlyCollection<EntityExpression>(expressions));

        es.ImportedAssemblies = new[] { typeof(Common.Extensions.ObjectExtensions).GetTypeInfo().Assembly.FullName! };
        es.ImportedNamespaces = new[] { typeof(Common.Extensions.ObjectExtensions).Namespace! };

        if (initialAttributes != null)
        {
            es.InitialAttributes = initialAttributes;
        }
            
        var loggerFactory = new LoggerFactory();

        var evalSrv = new EntityScriptingService(loggerFactory.CreateLogger<EntityScriptingService>());

        evalSrv.Load(new[] { es });
        return evalSrv;
    }
}