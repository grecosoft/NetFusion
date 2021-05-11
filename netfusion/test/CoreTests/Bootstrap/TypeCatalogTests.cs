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
    /// A list of filtered service implementations can each be registered as the same service type.
    /// When injecting a set of services into a dependent component all registered as the same service
    /// type, an enumeration of the service type is used.
    /// </summary>
    public class TypeCatalogTests
    {
        [Fact]
        public void CanRegisterSetOfFilteredTypes_AsSameServiceType()
        {
            // Arrange:
            var catalog = new TypeCatalog(new ServiceCollection(), 
                typeof(ComponentOne), 
                typeof(ComponentTwo));
           
            catalog.AsService<ICommonComponent>(
                t => t.Name.StartsWith("Component", StringComparison.Ordinal),
                ServiceLifetime.Scoped);

            // Act:
            var serviceProvider = catalog.Services.BuildServiceProvider();
            var services = serviceProvider.GetServices<ICommonComponent>().ToArray();
            
            // Assert:
            services.Should().HaveCount(2);
            services.OfType<ComponentOne>().Should().HaveCount(1);
            services.OfType<ComponentTwo>().Should().HaveCount(1);
        }

        /// <summary>
        /// The following tests scanning for a set of service implementation types where each
        /// matching type is registered as the implementation type.
        /// </summary>
        [Fact]
        public void CanRegisterSetOfFilteredTypes_AsImplementationType()
        {
            // Arrange:
            var catalog = new TypeCatalog(new ServiceCollection(), 
                typeof(ComponentOne), 
                typeof(ComponentTwo));
            
            catalog.AsSelf(
                t => t.Name.StartsWith("Component", StringComparison.Ordinal),
                ServiceLifetime.Scoped);
            
            // Act:
            var serviceProvider = catalog.Services.BuildServiceProvider();
            var s1 = serviceProvider.GetService<ComponentOne>();
            var s2 = serviceProvider.GetService<ComponentTwo>();
            
            // Assert:
            s1.Should().NotBeNull();
            s2.Should().NotBeNull();
        }

        /// <summary>
        /// The following shows scanning for a set of service implementation types and
        /// providing a ServiceDescriptor specifying how the service should be registered.
        /// </summary>
        [Fact]
        public void CanRegisterSetOfFilteredTypes_AsServiceDescriptor()
        {
            // Arrange:
            var catalog = new TypeCatalog(new ServiceCollection(), 
                typeof(ComponentOne), 
                typeof(ComponentTwo));

            catalog.AsDescriptor(
                t => t.Name.StartsWith("Component", StringComparison.Ordinal),
                t => ServiceDescriptor.Transient(t, t));
            
            // Act:
            var serviceProvider = catalog.Services.BuildServiceProvider();
            var s1 = serviceProvider.GetService<ComponentOne>();
            var s2 = serviceProvider.GetService<ComponentTwo>();
            
            // Assert:
            s1.Should().NotBeNull();
            s2.Should().NotBeNull();
        }

        /// <summary>
        /// Service implementation types can be registered as a service interface
        /// based on the convention where the implementation type implements an
        /// interface with the same name prefixed with and "I".
        /// </summary>
        [Fact]
        public void CanRegisterSetOfFilteredTypes_AsMatchingInterface()
        {
            // Arrange:
            var catalog = new TypeCatalog(new ServiceCollection(), 
                typeof(ComponentOne),
                typeof(ComponentTwo));

            catalog.AsImplementedInterface(
                _ => true,
                ServiceLifetime.Scoped);
            
            // Act:
            var serviceProvider = catalog.Services.BuildServiceProvider();
            var s1 = serviceProvider.GetService<IComponentOne>();
            var s2 = serviceProvider.GetService<IComponentTwo>();
            
            // Assert:
            s1.Should().NotBeNull();
            s2.Should().NotBeNull();
            
            
            var service = serviceProvider.GetService<IComponentTwo>();
            
            // Assert:
            service.Should().NotBeNull();
        }

        /// <summary>
        /// Will register all implementation types ending with a prefix as
        /// the service interface named the same as the implementation type
        /// prefixed with "I".
        /// </summary>
        [Fact]
        public void CanRegisterSetOfTypesWithNamedSuffix_AsMatchingInterface()
        {
            // Arrange:
            var catalog = new TypeCatalog(new ServiceCollection(), typeof(ComponentTwo));

            catalog.AsImplementedInterface("Two", ServiceLifetime.Scoped);
            
            // Act:
            var serviceProvider = catalog.Services.BuildServiceProvider();
            var service = serviceProvider.GetService<IComponentTwo>();
            
            // Assert:
            service.Should().NotBeNull();
        }

        /// <summary>
        /// The service collection delegates to the Microsoft IServiceCollection.
        /// </summary>
        [Fact]
        public void CanCreateCatalog_FromServiceCollection()
        {
            var services = new ServiceCollection();
            var catalog = services.CreateCatalog(new [] {typeof(ComponentOne)});
            
            catalog.Should().NotBeNull();
        }

        /// <summary>
        /// If one and only one service interface named as the implementation type
        /// prefixed with "I" can't be determined, an exception is raised.
        /// </summary>
        [Fact]
        public void ServiceInterface_MatchingConvention_MustBeFound()
        {
            // Arrange:
            var catalog = new TypeCatalog(new ServiceCollection(), typeof(ComponentNotMatchingConvention));
            
            // Act/Assert:
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                catalog.AsImplementedInterface(_ => true, ServiceLifetime.Transient);
            });

            ex.Message.Should().Contain("does not implement one and only one interface");
        }

        /// <summary>
        /// When registering a specific implementation type as a service type,
        /// the implementation type must implement the specified interface.
        /// </summary>
        [Fact]
        public void ImplementationType_MustBeAssignable_ToServiceType()
        {
            // Arrange:
            var catalog = new TypeCatalog(new ServiceCollection(), typeof(ComponentOne));
            
            // Act/Assert:
            var ex = Assert.Throws<InvalidCastException>(() =>
            {
                catalog.AsService<ISpecial>(_ => true, ServiceLifetime.Transient);
            });

            ex.Message.Should().Contain("not assignable to").And.Contain("Implementation Type");
        }
        
        /// <summary>
        /// When registering a specific implementation instance as a service type,
        /// the instance  must implement the specified interface.
        /// </summary>
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

        /// <summary>
        /// A single service implementation instance can be registered
        /// as multiple supporting service interfaces.
        /// </summary>
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