using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NetFusion.Web.Mvc.Metadata;

namespace WebTests.Rest.ApiMetadata
{
    public static class ApiMetadataExtensions
    {
        public static void AssertParamMeta<T>(this IEnumerable<ApiParameterMeta> paramsMeta, string name, 
            bool isOptional = false, 
            object defaultValue = null)
        {
            var paramMeta = paramsMeta.Single(p => p.ParameterName == name);
            paramMeta.Should().NotBeNull();
            paramMeta.IsOptional.Should().Be(isOptional);
            paramMeta.ParameterType.Should().Be(typeof(T));

            if (defaultValue != null)
            {
                paramMeta.DefaultValue.Should().Be(defaultValue);
            }
        }

        public static void AssertResponseMeta(this IEnumerable<ApiResponseMeta> responseMeta, int statusCode,
            Type responseType = null)
        {
            responseMeta.Where(m => m.Status == statusCode && m.ModelType == responseType)
                .Should().HaveCount(1);

        }
    }
}