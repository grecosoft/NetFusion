using System;

namespace NetFusion.Settings
{
    /// <summary>
    /// Attribute specified on an application setting to indicate the
    /// section name from which the setting instance should be populated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ConfigurationSectionAttribute : Attribute
    {
        /// <summary>
        /// The section name.  Can be a string or a string separated by the colon
        /// character for a nested name.
        /// </summary>
        public string SectionName { get; }

        /// <summary>
        /// Specifies the sections name use to load the application setting.
        /// </summary>
        /// <param name="sectionName">The section name.  Can be a string or a string separated by 
        /// the colon character for a nested name.</param>
        public ConfigurationSectionAttribute(string sectionName)
        {
            SectionName = sectionName ?? throw new ArgumentNullException(nameof(sectionName),
                "Settings configuration name cannot be null.");
        }
    }
}
