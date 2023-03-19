using System;
using NetFusion.Common.Base.Validation;
using NetFusion.Common.Extensions;

namespace NetFusion.Core.Settings;

/// <summary>
/// Exception thrown when configured settings fail validation.
/// </summary>
public class SettingsValidationException : Exception
{
    /// <summary>
    /// The type of the invalid settings class.
    /// </summary>
    public Type SettingsType { get; }
        
    /// <summary>
    /// The section path from which the settings were loaded.
    /// </summary>
    public string SettingsSection { get; }
        
    /// <summary>
    /// The details of the validation.
    /// </summary>
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