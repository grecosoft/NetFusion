using NetFusion.MongoDB.Configs;
using NetFusion.Settings;
using System.ComponentModel.DataAnnotations;

namespace NetFusion.Domain.MongoDB.Scripting
{
    [ConfigurationSection("integration:scriptMetadata")]
    public class EntityScriptMetaDb : MongoSettings
    {
        [Required (AllowEmptyStrings = false, ErrorMessage = "Collection Name is Required.")]
        public string CollectionName { get; set; }
    }
}
