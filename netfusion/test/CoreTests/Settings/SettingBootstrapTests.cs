//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Options;
//using NetFusion.Base.Validation;
//using NetFusion.Common.Extensions;
//using NetFusion.Settings;
//using NetFusion.Settings.Plugin;
//using NetFusion.Test.Modules;
//using Xunit;
//
//namespace CoreTests.Settings
//{
//    public class SettingBootstrapTests
//    {
//        /// <summary>
//        /// When normally adding configuration settings as option classes for a section, one needs to invoke
//        /// Configure on the ServiceCollection for each option and specify the associated configuration path.
//        /// The NetFusion.Plugin does it automatically during the bootstrap process by locating all classes
//        /// implementing IAppSettings and automatically configuring them using the section path determined
//        /// by the specified ConfigurationSection attributes.
//        /// </summary>
//        [Fact]
//        public void AllAppSettings_Registered_AsOptions()
//        {
//            // Arrange
//            var settingsModule = ModuleTestFixture.SetupModule<AppSettingsModule>(
//                typeof(TestSettingsOne), 
//                typeof(TestSettingsTwo));
//            
//            var services = new ServiceCollection();
//            
//            // Act:
//            settingsModule.RegisterServices(services);
//
//            // Assert: (every application setting was added to service-collection)
//            Assert.Equal(4, services.Count);
//
//            var serviceTypes = services.Select(s => s.ServiceType).ToArray();
//            
//            Assert.Contains(typeof(IOptionsChangeTokenSource<TestSettingsOne>), serviceTypes);
//            Assert.Contains(typeof(IConfigureOptions<TestSettingsOne>), serviceTypes);
//            Assert.Contains(typeof(IOptionsChangeTokenSource<TestSettingsTwo>), serviceTypes);
//            Assert.Contains(typeof(IConfigureOptions<TestSettingsTwo>), serviceTypes);
//        }
//
//        /// <summary>
//        /// To make the use of configuration options easier, the settings plug-in module also
//        /// registers each IAppSetting class within the service collection as a factory method
//        /// that, when invoked, will return the settings option.
//        /// </summary>
//        [Fact]
//        public void AllAppSettings_Registered_AsFactory()
//        {
//            // Arrange:
//            var catalog = CatalogTestFixture.Setup(typeof(TestSettingsOne));
//            var module = new AppSettingsModule();
//
//            // Act:
//            module.ScanAllOtherPlugins(catalog);
//            
//            // Assert:
//            Assert.Equal(1, catalog.Services.Count);
//            Assert.NotNull(catalog.Services.First().ImplementationFactory);
//            Assert.Equal(ServiceLifetime.Singleton, catalog.Services.First().Lifetime);
//        }
//
//        [Fact]
//        public void DependentComponent_HasSettingsInjected()
//        {
//            // Arrange:
//            var catalog = CatalogTestFixture.Setup(typeof(TestSettingsOne));
//            catalog.Services.AddOptions();
//            
//            var configBldr = new ConfigurationBuilder()
//                .AddInMemoryCollection(new Dictionary<string, string>
//                {
//                    { "A:B:Value1", "500" },
//                    { "A:B:Value2", "600" }
//                });
//
//            var module = ModuleTestFixture.SetupModule<AppSettingsModule>(configBldr, typeof(TestSettingsOne));
//            
//            // ... Add service with settings dependency:
//            catalog.Services.AddScoped<DependentComponent>();
//            
//            // Act:
//            module.RegisterServices(catalog.Services);
//            module.ScanAllOtherPlugins(catalog);
//            
//            // Assert:
//            var serviceProvider = catalog.Services.BuildServiceProvider();
//            var component = serviceProvider.GetRequiredService<DependentComponent>();
//            
//            Assert.NotNull(component.InjectedSettings);
//            Assert.Equal(500, component.InjectedSettings.Value1);
//            Assert.Equal(600, component.InjectedSettings.Value2);
//        }
//
//        [Fact]
//        public void WhenAppSetting_Injected_StateValidated()
//        {
//            // Arrange:
//            var catalog = CatalogTestFixture.Setup(typeof(TestSettingsInvalid));
//            catalog.Services.AddOptions();
//            
//            var configBldr = new ConfigurationBuilder()
//                .AddInMemoryCollection(new Dictionary<string, string>
//                {
//                    { "A:B:Value1", "900" },
//                    { "A:B:Value2", "600" }
//                });
//
//            var module = ModuleTestFixture.SetupModule<AppSettingsModule>(configBldr, typeof(TestSettingsInvalid));
//            
//            // ... Add service with settings dependency:
//            catalog.Services.AddScoped<DependentComponentInvalid>();
//            
//            // Act:
//            module.RegisterServices(catalog.Services);
//            module.ScanAllOtherPlugins(catalog);
//            
//            // Assert:
//            var serviceProvider = catalog.Services.BuildServiceProvider();
//
//            Assert.Throws<ValidationResultException>(() =>
//            {
//                serviceProvider.GetRequiredService<DependentComponentInvalid>();
//            });           
//        }
//
//        [Fact]
//        public void AppSettingsAndPath_Logged()
//        {
//            // Arrange
//            var module = ModuleTestFixture.SetupModule<AppSettingsModule>(
//                typeof(TestSettingsOne));
//            
//            // Act:
//            var log = new Dictionary<string, object>();
//            module.Log(log);
//            
//            // Assert:
//            Assert.Single(log);
//            var logItem = log.First();
//
//            Assert.Equal("Application:Settings", logItem.Key);
//
//            var logItems = (IEnumerable<object>)logItem.Value;
//            object settingLogItem = logItems.ElementAt(0);
//
//            var logValues = settingLogItem.ToDictionary();
//            
//            Assert.Equal(typeof(TestSettingsOne).AssemblyQualifiedName, logValues["SettingsClass"]);
//            Assert.Equal(SettingsExtensions.GetSectionPath<TestSettingsOne>(), logValues["SectionPath"]);
//        }
//
//
//        [ConfigurationSection("A:B")]
//        public class TestSettingsOne: IAppSettings
//        {
//            public int Value1 { get; set; } = 10;
//            public int Value2 { get; set; } = 100;
//        }
//        
//        [ConfigurationSection("A:B")]
//        public class TestSettingsInvalid: IAppSettings,
//            IValidatableType
//        {
//            public int Value1 { get; set; } = 10;
//            public int Value2 { get; set; } = 100;
//            
//            public void Validate(IObjectValidator validator)
//            {
//                validator.Verify(Value1 <= Value2, "Value1 must be greater than Value2");
//            }
//        }
//
//        [ConfigurationSection("X:Y")]
//        public class TestSettingsTwo : IAppSettings
//        {
//            
//        }
//
//        public class DependentComponent
//        {
//            public TestSettingsOne InjectedSettings { get; }
//            
//            public DependentComponent(TestSettingsOne settings)
//            {
//                InjectedSettings = settings;
//            }
//        }
//        
//        public class DependentComponentInvalid
//        {
//            public TestSettingsInvalid InjectedSettings { get; }
//            
//            public DependentComponentInvalid(TestSettingsInvalid settings)
//            {
//                InjectedSettings = settings;
//            }
//        }
//    }
//}