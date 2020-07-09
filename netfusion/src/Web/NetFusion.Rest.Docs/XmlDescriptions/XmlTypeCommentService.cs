using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using NetFusion.Common.Extensions.Types;
using NetFusion.Rest.Docs.Core;
using NetFusion.Rest.Docs.Models;
using NetFusion.Rest.Resources;

namespace NetFusion.Rest.Docs.XmlDescriptions
{
    public class XmlTypeCommentService : ITypeCommentService
    {
        private static Type[] PrimitiveTypes { get; } = {typeof(string), typeof(DateTime)};

        private readonly IXmlCommentService _xmlComments;

        public XmlTypeCommentService(IXmlCommentService xmlComments)
        {
            _xmlComments = xmlComments;
        }

        public ApiResourceDoc GetResourceDoc(Type resourceType)
        {
            XPathNavigator xmlCommentDoc = _xmlComments.GetXmlCommentsForTypesAssembly(resourceType);

            return xmlCommentDoc == null ? null : BuildResourceDoc(resourceType);
        }

        private ApiResourceDoc BuildResourceDoc(Type resourceType)
        {
            var resourceDoc = new ApiResourceDoc
            {
                Description = _xmlComments.GetTypeComments(resourceType),
                ResourceName = resourceType.GetExposedResourceName(),
                ResourceType = resourceType
            };

            foreach (PropertyInfo propInfo in GetDescribableProperties(resourceType))
            {
                var propDoc = new ApiPropertyDoc
                {
                    Name = propInfo.Name,
                    IsRequired = IsMarkedRequired(propInfo) || !IsNullableProperty(propInfo),
                    Description = _xmlComments.GetTypeMemberComments(propInfo)
                };
                
                if (IsPrimitiveProperty(propInfo))
                {
                    propDoc.Type = propInfo.PropertyType.GetJsTypeName();
                    
                }
                else if (IsEnumerableProperty(propInfo))
                {
                    propDoc.IsArray = true;

                    Type itemType = GetEnumerableType(propInfo);
                    if (IsPrimitiveType(itemType))
                    {
                        propDoc.Type = itemType.GetJsTypeName();
                    }
                    else
                    {
                        propDoc.Type = BuildResourceDoc(itemType);
                        propDoc.IsObject = true;
                    }
                }
                else if (propInfo.PropertyType.IsClass)
                {
                    propDoc.IsObject = true;
                    propDoc.Type = BuildResourceDoc(propInfo.PropertyType);
                }
                
                resourceDoc.Properties.Add(propDoc);
            }

            return resourceDoc;
        }


        private static IEnumerable<PropertyInfo> GetDescribableProperties(Type type) => 
            type.GetProperties().Where(p => p.CanRead);


        private static bool IsPrimitiveProperty(PropertyInfo propertyInfo)
        {
            Type propType = propertyInfo.PropertyType;

            return propType.IsPrimitive || PrimitiveTypes.Contains(propType);
        }

        private static bool IsPrimitiveType(Type type)
        {
            return type.IsPrimitive || PrimitiveTypes.Contains(type);
        }

        private static bool IsNullableProperty(PropertyInfo propertyInfo)
        {
            Type propType = propertyInfo.PropertyType;

            return propType.IsClass ||
                (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>));
        }


        private static bool IsMarkedRequired(PropertyInfo propertyInfo) =>
            propertyInfo.GetCustomAttribute<RequiredAttribute>() != null;


        private static bool IsEnumerableProperty(PropertyInfo propertyInfo)
        {
            Type propType = propertyInfo.PropertyType;

            return propType.IsArray || propType.IsSubclassOf(typeof(IEnumerable));
        }

        private static Type GetEnumerableType(PropertyInfo propertyInfo)
        {
            Type propType = propertyInfo.PropertyType;
            

            if (propType.IsArray)
            {
                return propType.GetElementType();
            }

            if (propType.IsSubclassOf(typeof(IEnumerable<>)))
            {
                return propType.GetGenericArguments()[0];
            }

            return null;
         
        }
    }
}