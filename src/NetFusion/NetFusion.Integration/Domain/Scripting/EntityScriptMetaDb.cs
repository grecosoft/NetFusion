using NetFusion.MongoDB.Configs;
using System.ComponentModel.DataAnnotations;

namespace NetFusion.Integration.Domain.Scripting
{
    public class EntityScriptMetaDb : MongoSettings
    {
        [Required (AllowEmptyStrings = false, ErrorMessage = "Collection Name is Required.")]
        public string CollectionName { get; set; }
    }
}
