using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NetFusion.Common.Extensions.Expressions;

public static class MethodCallExtensions
{
    public static MethodInfo? GetCallMethodInfo(this LambdaExpression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        UnaryExpression? unaryExpression = expression.Body as UnaryExpression;

        MethodCallExpression? methodCallExpression =
            unaryExpression?.Operand as MethodCallExpression;

        ConstantExpression? constantExpression =
            methodCallExpression?.Object as ConstantExpression;

        MethodInfo? methodInfo = null;
        if (constantExpression != null)
        {
            methodInfo = constantExpression.Value as MethodInfo;
        }
        else if (methodCallExpression != null)
        {
            constantExpression = methodCallExpression.Arguments
                .FirstOrDefault(a =>
                    a.Type == typeof(MethodInfo) &&
                    a.NodeType == ExpressionType.Constant) as ConstantExpression;

            methodInfo = constantExpression?.Value as MethodInfo;
        }
        return methodInfo;
    }
}