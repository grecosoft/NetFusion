using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Test.Modules;
using Xunit;

namespace CoreTests.Bootstrap
{
    public class TypeCatalogTests
    {
        [Fact]
        public void CanRegisterSetOfFilteredTypes_AsSameServceType()
        {
            var catalog = CatalogTestFixture.Setup(typeof(ComponentOne), typeof(ComponentTwo));

            catalog.AsService<ICommonComponent>(
                t => t.Name.StartsWith("Component", System.StringComparison.Ordinal),
                ServiceLifetime.Scoped);
            
            Assert.Equal(2, catalog.Services.Count);
            Assert.True(catalog.Services.All(s => s.ServiceType == typeof(ICommonComponent)));
            Assert.True(catalog.Services.All(s => s.Lifetime == ServiceLifetime.Scoped));
            Assert.Contains(catalog.Services, s => s.ImplementationType == typeof(ComponentOne));
            Assert.Contains(catalog.Services, s => s.ImplementationType == typeof(ComponentTwo));
        }

        [Fact]
        public void CanRegisterSetOfFilteredTypes_AsServiceType()
        {
            var catalog = CatalogTestFixture.Setup(typeof(ComponentOne), typeof(ComponentTwo));

            catalog.AsSelf(
                t => t.Name.StartsWith("Component", System.StringComparison.Ordinal),
                ServiceLifetime.Scoped);
            
            Assert.Equal(2, catalog.Services.Count);
            Assert.True(catalog.Services.All(s => s.Lifetime == ServiceLifetime.Scoped));
            Assert.Contains(catalog.Services, s => s.ServiceType == typeof(ComponentOne) && s.ImplementationType == typeof(ComponentOne));
            Assert.Contains(catalog.Services, s => s.ServiceType == typeof(ComponentTwo) && s.ImplementationType == typeof(ComponentTwo));
        }

        [Fact]
        public void CanRegisterSetOfFilteredTypes_AsServiceDescriptor()
        {
            var catalog = CatalogTestFixture.Setup(typeof(ComponentOne), typeof(ComponentTwo));

            catalog.AsDescriptor(
                t => t.Name.StartsWith("Component", System.StringComparison.Ordinal),
                t => ServiceDescriptor.Transient(t, t));
            
            Assert.Equal(2, catalog.Services.Count);
            Assert.True(catalog.Services.All(s => s.Lifetime == ServiceLifetime.Transient));
            Assert.Contains(catalog.Services, s => s.ImplementationType == typeof(ComponentOne));
            Assert.Contains(catalog.Services, s => s.ImplementationType == typeof(ComponentTwo));
        }

        [Fact]
        public void CanRegisterSetOfFilteredTypes_AsAllSupportedInterfaces()
        {
            var catalog = CatalogTestFixture.Setup(typeof(ComponentTwo));

            catalog.AsImplementedInterface(
                _ => true,
                ServiceLifetime.Scoped);
            
            Assert.Equal(1, catalog.Services.Count);
            Assert.True(catalog.Services.All(s => s.Lifetime == ServiceLifetime.Scoped));
            Assert.Contains(catalog.Services, s => s.ServiceType == typeof(IComponentTwo)); 
        }

        [Fact]
        public void CanRegisterSetOfTypesWithNamedSuffix_AsAllSupportedInterfaces()
        {
            var catalog = CatalogTestFixture.Setup(typeof(ComponentOne), typeof(ComponentTwo));

            catalog.AsImplementedInterface("Two", ServiceLifetime.Scoped);
            
            Assert.Equal(1, catalog.Services.Count);
            Assert.True(catalog.Services.All(s => s.ImplementationType == typeof(ComponentTwo)));
            Assert.True(catalog.Services.All(s => s.Lifetime == ServiceLifetime.Scoped));
            Assert.Contains(catalog.Services, s => s.ServiceType == typeof(IComponentTwo));
        }


        public interface ICommonComponent
        {
            
        }

        public interface IComponentOne
        {

        }

        public interface IComponentTwo
        {

        }

        public class ComponentOne : ICommonComponent,
            IComponentOne
        {
            
        }

        public class ComponentTwo : ICommonComponent, ISpecial,
            IComponentTwo
        {
            
        }

        public interface ISpecial
        {
            
        }
    }
}