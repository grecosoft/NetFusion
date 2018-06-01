using System;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Dependencies;

namespace NetFusion.Test.Modules
{
    public static class CatalogTestFixture
    {
        public static TypeCatalog Setup(params Type[] types)
        {
            return new TypeCatalog(new ServiceCollection(), types);
        }
    }
}