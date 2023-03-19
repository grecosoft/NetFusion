using System;

namespace NetFusion.Common.Extensions.Types;

/// <summary>
/// Extensions for mapping .NET type to common client types.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Determines the JavaScript type corresponding to a .NET type.
    /// </summary>
    /// <param name="type">The .NET type.</param>
    /// <returns>The corresponding JavaScript type.</returns>
    public static string GetJsTypeName(this Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        switch (type.Name)
        {
            case "Byte":
            case "SByte":
            case "Decimal":
            case "Double":
            case "Single":
            case "Int32":
            case "UInt32":
            case "Int64":
            case "UInt64":
            case "Int16":
            case "UInt16":
                return "Number";

            case "Boolean":
                return "Boolean";

            case "String":
            case "Char":
                return "String";

            case "DateTime":
                return "Date";
            default: 
                return "Object";
        }
    }
}