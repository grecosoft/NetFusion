using System.Linq.Expressions;
using System.Reflection;

namespace NetFusion.Messaging.Routing;

/// <summary>
/// Contains details of how a message is routed to its associated consumer.
/// Metadata can also be associated with the route.  Such metadata can be used
/// to subscribe the consumer to an external source such as a message-broker
/// or alter how the message is handled.
/// </summary>
public abstract class MessageRoute
{
    internal Type MessageType { get; }
    internal Type? ResultType { get; }
    internal Type? ConsumerType { get; private set; }
    internal MethodInfo? HandlerMethodInfo { get; private set; }
    
    internal Func<object, object[], object?>? Invoker { get; private set; } 
    internal IRouteMeta? RouteMeta { get; private set; }

    /// <summary>
    /// Creates a route for a message type not having a result.
    /// </summary>
    /// <param name="messageType">The type of the message.</param>
    protected MessageRoute(Type messageType)
    {
        MessageType = messageType;
    }

    /// <summary>
    /// Creates a route for a message type having a result.
    /// </summary>
    /// <param name="messageType">The type of message.</param>
    /// <param name="resultType">The associated message result.</param>
    protected MessageRoute(Type messageType, Type resultType)
    {
        MessageType = messageType;
        ResultType = resultType;
    }

    /// <summary>
    /// Sets the message handler, based on a typed lambda expression, to be
    /// invoked when a message is received.
    /// </summary>
    /// <param name="consumer">Lambda expression for the consumer's message handler.</param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    protected void SetConsumer<TConsumer>(LambdaExpression consumer)
    {
        ConsumerType = typeof(TConsumer);
        HandlerMethodInfo = GetHandlerMethodInfo(consumer);
        Invoker = CreateInvoker(ConsumerType, HandlerMethodInfo);
    }

    /// <summary>
    /// Sets the message handler to be invoked when a message is received.
    /// </summary>
    /// <param name="handlerMethodInfo">Method info for the consumer's message handler.</param>
    protected void SetConsumer(MethodInfo handlerMethodInfo)
    {
        ConsumerType = handlerMethodInfo.DeclaringType!;
        HandlerMethodInfo = handlerMethodInfo;
        Invoker = CreateInvoker(ConsumerType, HandlerMethodInfo);
    }

    private static MethodInfo GetHandlerMethodInfo(LambdaExpression consumer)
    {
        var unaryExpression = (UnaryExpression)consumer.Body;
        var callExpression = (MethodCallExpression)unaryExpression.Operand;
        var constantExpression = (ConstantExpression?)callExpression.Object;
        
        var methodInfo = (MethodInfo?)constantExpression?.Value;
        if (methodInfo == null)
        {
            throw new InvalidOperationException("MethodInfo could not be determined for message consumer handler");
        }

        return methodInfo;
    }

    /// <summary>
    /// Creates and populates a new route-meta instance associated with the route.
    /// </summary>
    /// <param name="routeMeta">Route metadata associated with route.</param>
    /// <param name="configure">Action called to initialize the metadata.</param>
    /// <typeparam name="TRouteMeta">The type of the route-meta associated with route.</typeparam>
    protected void ConfigureRouteMeta<TRouteMeta>(TRouteMeta routeMeta, Action<TRouteMeta> configure)
        where TRouteMeta : IRouteMeta, new()
    {
        configure(routeMeta);
        RouteMeta = routeMeta;
    }
    
    // Creates a delegate used to invoke the consumer's message handler.  
    // This results in execution times faster than using MethodInfo.Invoke 
    // or calling DynamicInvoke on a created expression and is close to
    // the execution time for a direct method call.
    private static Func<object, object[], object> CreateInvoker(Type type, MethodInfo method)
    {
        CreateParamsExpressions(method, out ParameterExpression argsExp, out Expression[] paramsExps);

        var targetExp = Expression.Parameter(typeof(object), "target");
        var castTargetExp = Expression.Convert(targetExp, type);
        var invokeExp = Expression.Call(castTargetExp, method, paramsExps);

        LambdaExpression lambdaExp;
            
        if (method.ReturnType != typeof(void))
        {
            var resultExp = Expression.Convert(invokeExp, typeof(object));
            lambdaExp = Expression.Lambda(resultExp, targetExp, argsExp);
        }
        else
        {
            var constExp = Expression.Constant(null, typeof(object));
            var blockExp = Expression.Block(invokeExp, constExp);
            lambdaExp = Expression.Lambda(blockExp, targetExp, argsExp);
        }

        var lambda = lambdaExp.Compile();
        return (Func<object, object[], object>)lambda;
    }

    private static void CreateParamsExpressions(MethodBase method, out ParameterExpression argsExp, 
        out Expression[] paramsExps)
    {
        var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

        argsExp = Expression.Parameter(typeof(object[]), "args");
        paramsExps = new Expression[parameterTypes.Length];

        for (var i = 0; i < parameterTypes.Length; i++)
        {
            var constExp = Expression.Constant(i, typeof(int));
            var argExp = Expression.ArrayIndex(argsExp, constExp);
            paramsExps[i] = Expression.Convert(argExp, parameterTypes[i]);
        }
    }
}