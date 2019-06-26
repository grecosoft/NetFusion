using System;
using NetFusion.Base.Validation;
using NetFusion.Common.Extensions;

namespace NetFusion.Settings
{
    public class SettingsValidationException : Exception
    {
        public Type SettingsType { get; }
        public string SettingsSection { get; }
        public ValidationResultSet ValidationResults { get; }
        
        public SettingsValidationException(Type settingsType, string settingSection, 
            ValidationResultSet validationResults)
            : base($"Type:{settingsType}; Section: {settingSection}; Results: {validationResults.ToIndentedJson()}")
        {
            SettingsType = settingsType;
            SettingsSection = settingSection;
            ValidationResults = validationResults;
        }
    }
}