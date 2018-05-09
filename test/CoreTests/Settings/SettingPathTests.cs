using NetFusion.Settings;
using Xunit;

namespace CoreTests.Settings
{
    public class SettingPathTests
    {
        [Fact]
        public void SectionNameDetermined_ByClassStructure()
        {
            var appSettings = new DerivedSettingClass();
            var path = appSettings.GetSectionPath();
                
            Assert.Equal("A:B:C:D", path);
        }

        [Fact]
        public void SectionNameOptional_OnDerivedSetting()
        {
            var appSettings = new BaseSettingClass();
            var path = appSettings.GetSectionPath();
                
            Assert.Equal(path, "A:B");
        }
        
        [Fact]
        public void SectionName_NotRequired()
        {
            var appSettings = new SettingsWithNoPath();
            var path = appSettings.GetSectionPath();
                
            Assert.Equal(path, string.Empty);
        }

        [ConfigurationSection("A:B")]
        private class RootSettingClass : IAppSettings
        {
            
        }

        private class BaseSettingClass : RootSettingClass
        {
            
        }

        [ConfigurationSection("C:D")]
        private class DerivedSettingClass : BaseSettingClass
        {
            
        }

        private class SettingsWithNoPath : IAppSettings
        {
            
        }
    }
}