using System.Collections.Generic;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.XmlComments;

namespace NetFusion.Rest.Docs.Plugin.Configs
{
    public class DocDescriptionConfig : IPluginConfig
    {
        private readonly List<IDocDescription> _descriptions;

        public DocDescriptionConfig()
        {
            _descriptions = new List<IDocDescription>
            {
                // TODO:  Make list of types and create new instances to reduce
                // chance of multiple threads using context regardless if call
                // being initiated from a Lazy evaluation.
                new XmlActionComments(),
                new XmlParamComments(),
                new XmlResultComments()
            };
            
            Descriptions = _descriptions.AsReadOnly();
        }
        
        public IReadOnlyCollection<IDocDescription> Descriptions { get; }
    }
}