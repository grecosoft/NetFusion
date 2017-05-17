using NetFusion.MongoDB.Configs;
using System.ComponentModel.DataAnnotations;

namespace NetFusion.Domain.MongoDB.Scripting
{
    public class EntityScriptMetaDb : MongoSettings
    {
        [Required (AllowEmptyStrings = false, ErrorMessage = "Collection Name is Required.")]
        public string CollectionName { get; set; }
    }
}
