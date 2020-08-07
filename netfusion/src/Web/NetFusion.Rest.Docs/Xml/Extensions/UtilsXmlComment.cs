using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetFusion.Rest.Docs.Xml.Extensions
{
    /// <summary>
    /// Utility class containing methods used to build member type names for types
    /// stored within XML generated comment files.  These member type names are used
    /// to locate the documentation within the XML file.
    /// </summary>
    public class UtilsXmlComment
    {
        public static string GetMemberNameForMethod(MethodInfo method)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));
            
            var builder = new StringBuilder("M:");
            builder.Append(QualifiedNameFor(method.DeclaringType));
            builder.Append($".{method.Name}");

            var parameters = method.GetParameters();
            if (!parameters.Any())  return builder.ToString();
            
            var parametersNames = parameters.Select(p => p.ParameterType.IsGenericParameter
                ? $"`{p.ParameterType.GenericParameterPosition}"
                : QualifiedNameFor(p.ParameterType, true));
            
            builder.Append($"({string.Join(",", parametersNames)})");

            return builder.ToString();
        }

        public static string GetMemberNameForType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            
            var builder = new StringBuilder("T:");
            builder.Append(QualifiedNameFor(type));

            return builder.ToString();
        }

        public static string GetMemberNameForFieldOrProperty(MemberInfo fieldOrPropertyInfo)
        {
            if (fieldOrPropertyInfo == null) throw new ArgumentNullException(nameof(fieldOrPropertyInfo));
            
            var builder = new StringBuilder(((fieldOrPropertyInfo.MemberType & MemberTypes.Field) != 0) ? "F:" : "P:");
            builder.Append(QualifiedNameFor(fieldOrPropertyInfo.DeclaringType));
            builder.Append($".{fieldOrPropertyInfo.Name}");

            return builder.ToString();
        }

        private static string QualifiedNameFor(Type type, bool expandGenericArgs = false)
        {
            if (type.IsArray)
                return $"{QualifiedNameFor(type.GetElementType(), expandGenericArgs)}[]";

            var builder = new StringBuilder();

            if (!string.IsNullOrEmpty(type.Namespace))
                builder.Append($"{type.Namespace}.");

            if (type.IsNested)
            {
                builder.Append($"{string.Join(".", GetNestedTypeNames(type))}.");
            }

            if (type.IsConstructedGenericType && expandGenericArgs)
            {
                var nameSansGenericArgs = type.Name.Split('`').First();
                builder.Append(nameSansGenericArgs);

                var genericArgsNames = type.GetGenericArguments().Select(t => t.IsGenericParameter
                    ? $"`{t.GenericParameterPosition}"
                    : QualifiedNameFor(t, true));

                builder.Append($"{{{string.Join(",", genericArgsNames)}}}");
            }
            else
            {
                builder.Append(type.Name);
            }

            return builder.ToString();
        }

        private static IEnumerable<string> GetNestedTypeNames(Type type)
        {
            if (!type.IsNested || type.DeclaringType == null) yield break;

            foreach (var nestedTypeName in GetNestedTypeNames(type.DeclaringType))
            {
                yield return nestedTypeName;
            }

            yield return type.DeclaringType.Name;
        }
    }
}