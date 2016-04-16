using NetFusion.Common.Extensions;
using NetFusion.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Settings.Mongo
{
    /// <summary>
    /// The MongoDB entity mapping used to store and retrieve derived AppSettings
    /// from a collection.
    /// </summary>
    public class AppSettingMapping : EntityClassMap<AppSettings>
    {
        public AppSettingMapping()
        {
            this.AutoMap();
            this.MapStringObjectIdProperty(p => p.AppSettingsId);
        }

        public override void AddKnownPluginTypes(IEnumerable<Type> pluginTypes)
        {
            var appSettings = pluginTypes.Where(t =>
                t.IsDerivedFrom<IAppSettings>() && !t.IsAbstract && !t.IsInterface);

            appSettings.ForEach(s => this.AddKnownType(s));

        }
    }
}
