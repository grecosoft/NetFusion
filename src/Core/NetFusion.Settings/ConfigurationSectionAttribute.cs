using NetFusion.Common;
using System;

namespace NetFusion.Settings
{
    /// <summary>
    /// Attribute specified on an application setting to indicate the
    /// section name from which the setting instance should be populated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ConfigurationSectionAttribute : Attribute
    {
        public string SectionName { get; }

        public ConfigurationSectionAttribute(string sectionName)
        {
            Check.NotNull(sectionName, nameof(sectionName));
            this.SectionName = sectionName;
        }
    }
}
