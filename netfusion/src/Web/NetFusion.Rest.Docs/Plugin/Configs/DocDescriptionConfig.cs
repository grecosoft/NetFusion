using System.Collections.Generic;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Docs.Core;
using NetFusion.Rest.Docs.Descriptions.Comments;

namespace NetFusion.Rest.Docs.Plugin.Configs
{
    public class DocDescriptionConfig : IPluginConfig
    {
        private readonly List<IDocDescription> _descriptions;

        public DocDescriptionConfig()
        {
            _descriptions = new List<IDocDescription>
            {
                new XmlClassComments(),
                new XmlParamComments(),
                new XmlResultComments()
            };
            
            Descriptions = _descriptions.AsReadOnly();
        }
        
        public IReadOnlyCollection<IDocDescription> Descriptions { get; }
    }
}