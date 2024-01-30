using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Common.Extensions.Types;
using NetFusion.Web.Rest.Docs.Core;
using NetFusion.Web.Rest.Docs.Models;
using NetFusion.Web.Rest.Resources;

namespace NetFusion.Web.Rest.Docs.Xml.Services;

/// <summary>
/// Provides an implementation of the ITypeCommentService used to document types
/// based on XML comment files.
/// </summary>
public class XmlTypeCommentService(IXmlCommentService xmlComments) : ITypeCommentService
{
    private readonly IXmlCommentService _xmlComments = xmlComments ?? 
        throw new ArgumentNullException(nameof(xmlComments));

    public ApiResourceDoc GetResourceDoc(Type resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        XPathNavigator xmlCommentDoc = _xmlComments.GetXmlCommentsForTypesAssembly(resourceType);
        return xmlCommentDoc == null ? null : BuildResourceDoc(resourceType);
    }

    private ApiResourceDoc BuildResourceDoc(Type resourceType)
    {
        var resourceDoc = new ApiResourceDoc
        {
            Description = _xmlComments.GetTypeComments(resourceType),
            ResourceName = resourceType.GetResourceName(),
            ResourceType = resourceType
        };
            
        AddPropertyComments(resourceDoc);
        return resourceDoc;
    }

    private void AddPropertyComments(ApiResourceDoc resourceDoc)
    {
        foreach (PropertyInfo propInfo in GetDescribableProperties(resourceDoc.ResourceType))
        {
            var propDoc = new ApiPropertyDoc
            {
                Name = propInfo.Name,
                IsRequired = propInfo.IsMarkedRequired() || !propInfo.IsNullable(),
                Description = _xmlComments.GetTypeMemberComments(propInfo)
            };
                
            if (propInfo.IsEnumerable())
            {
                propDoc.IsArray = true;

                Type itemType = propInfo.GetEnumerableType();
                if (itemType.IsBasicType())
                {    
                    propDoc.Type = itemType.GetJsTypeName();
                }
                else
                {
                    propDoc.Type = itemType.GetJsTypeName();
                    propDoc.ResourceDoc = BuildResourceDoc(itemType);
                }
            }
            else if (propInfo.IsBasicType())
            {
                propDoc.Type = propInfo.PropertyType.GetJsTypeName();
            }
            else if (propInfo.PropertyType.IsClass)
            {
                propDoc.Type = propInfo.PropertyType.GetJsTypeName();
                propDoc.ResourceDoc = BuildResourceDoc(propInfo.PropertyType);
            }
                
            resourceDoc.Properties.Add(propDoc);
        }
    }
        
    private static IEnumerable<PropertyInfo> GetDescribableProperties(Type type) => 
        type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
            .Where(p => p.CanRead);
}