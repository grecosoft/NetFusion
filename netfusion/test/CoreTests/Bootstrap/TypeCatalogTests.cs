using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Catalog;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
using Xunit;

namespace CoreTests.Bootstrap
{
    /// <summary>
    /// Validates scanning for service implementations that are registered as services with the Microsoft
    /// IServiceCollection.
    /// </summary>
    public class TypeCatalogTests
    {
        [Fact]
        public void CanRegisterSetOfFilteredTypes_AsSameServiceType()
        {
            var catalog = new TypeCatalog(new ServiceCollection(), 
                typeof(ComponentOne), 
                typeof(ComponentTwo));
           
            catalog.AsService<ICommonComponent>(
                t => t.Name.StartsWith("Component", StringComparison.Ordinal),
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
            var catalog = new TypeCatalog(new ServiceCollection(), 
                typeof(ComponentOne), 
                typeof(ComponentTwo));

            catalog.AsSelf(
                t => t.Name.StartsWith("Component", StringComparison.Ordinal),
                ServiceLifetime.Scoped);
            
            Assert.Equal(2, catalog.Services.Count);
            Assert.True(catalog.Services.All(s => s.Lifetime == ServiceLifetime.Scoped));
            Assert.Contains(catalog.Services, s => s.ServiceType == typeof(ComponentOne) && s.ImplementationType == typeof(ComponentOne));
            Assert.Contains(catalog.Services, s => s.ServiceType == typeof(ComponentTwo) && s.ImplementationType == typeof(ComponentTwo));
        }

        [Fact]
        public void CanRegisterSetOfFilteredTypes_AsServiceDescriptor()
        {
            var catalog = new TypeCatalog(new ServiceCollection(), 
                typeof(ComponentOne), 
                typeof(ComponentTwo));

            catalog.AsDescriptor(
                t => t.Name.StartsWith("Component", StringComparison.Ordinal),
                t => ServiceDescriptor.Transient(t, t));
            
            Assert.Equal(2, catalog.Services.Count);
            Assert.True(catalog.Services.All(s => s.Lifetime == ServiceLifetime.Transient));
            Assert.Contains(catalog.Services, s => s.ImplementationType == typeof(ComponentOne));
            Assert.Contains(catalog.Services, s => s.ImplementationType == typeof(ComponentTwo));
        }

        [Fact]
        public void CanRegisterSetOfFilteredTypes_AsMatchingInterface()
        {
            var catalog = new TypeCatalog(new ServiceCollection(), typeof(ComponentTwo));

            catalog.AsImplementedInterface(
                _ => true,
                ServiceLifetime.Scoped);
            
            Assert.Equal(1, catalog.Services.Count);
            Assert.True(catalog.Services.All(s => s.Lifetime == ServiceLifetime.Scoped));
            Assert.Contains(catalog.Services, s => s.ServiceType == typeof(IComponentTwo)); 
        }

        [Fact]
        public void CanRegisterSetOfTypesWithNamedSuffix_AsMatchingInterface()
        {
            var catalog = new TypeCatalog(new ServiceCollection(), typeof(ComponentTwo));

            catalog.AsImplementedInterface("Two", ServiceLifetime.Scoped);
            
            Assert.Equal(1, catalog.Services.Count);
            Assert.True(catalog.Services.All(s => s.ImplementationType == typeof(ComponentTwo)));
            Assert.True(catalog.Services.All(s => s.Lifetime == ServiceLifetime.Scoped));
            Assert.Contains(catalog.Services, s => s.ServiceType == typeof(IComponentTwo));
        }

        [Fact]
        public void CanCreateCatalog_FromServiceCollection()
        {
            var services = new ServiceCollection();
            var catalog = services.CreateCatalog(new [] {typeof(ComponentOne)});
            
            catalog.Should().NotBeNull();
            services.Should().BeEmpty();
        }

        [Fact]
        public void ServiceInterface_MatchingConvention_MustBeFound()
        {
            var catalog = new TypeCatalog(new ServiceCollection(), typeof(ComponentNotMatchingConvention));
            
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                catalog.AsImplementedInterface(_ => true, ServiceLifetime.Transient);
            });

            ex.Message.Should().Contain("does not implement one and only one interface");
        }

        [Fact]
        public void ImplementationType_MustBeAssignable_ToServiceType()
        {
            var catalog = new TypeCatalog(new ServiceCollection(), typeof(ComponentOne));
            
            var ex = Assert.Throws<InvalidCastException>(() =>
            {
                catalog.AsService<ISpecial>(_ => true, ServiceLifetime.Transient);
            });

            ex.Message.Should().Contain("not assignable to").And.Contain("Implementation Type");
        }
        
        [Fact]
        public void ImplementationInstance_MustBeAssignable_ToServiceType()
        {
            var catalog = new TypeCatalog(new ServiceCollection(), typeof(ComponentOne));
            
            var ex = Assert.Throws<InvalidCastException>(() =>
            {
                catalog.AsDescriptor(
                    _ => true,
                    st => ServiceDescriptor.Singleton(typeof(ISpecial), st.CreateInstance()));
            });

            ex.Message.Should().Contain("not assignable to").And.Contain("Implementation Instance");
        }

        [Fact]
        public void CanRegisterService_ProvidingMultiple_Behaviors()
        {
            var services = new ServiceCollection();
            var serviceModule = new PluginModule();

            var behaviors = serviceModule.GetType().GetInterfaces()
                .Where(it => it.CanAssignTo<IPluginModuleService>())
                .ToArray();

            services.AddSingleton(behaviors, serviceModule);

            var provider = services.BuildServiceProvider();
            var behaviorA = provider.GetRequiredService<IProvideBehaviorA>();
            var behaviorB = provider.GetRequiredService<IProvideBehaviorB>();

            behaviorA.Should().Be(behaviorB);
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

        public class ComponentNotMatchingConvention : ISpecial
        {
            
        }

        public interface IProvideBehaviorA : IPluginModuleService
        {
            
        }
        
        public interface IProvideBehaviorB : IPluginModuleService
        {
            
        }
        
        public class PluginModule : IProvideBehaviorA, IProvideBehaviorB
        {
            
        }
    }
}