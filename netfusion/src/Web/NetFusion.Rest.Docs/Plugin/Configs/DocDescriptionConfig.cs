using System;
using System.Collections.Generic;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.XmlComments;

namespace NetFusion.Rest.Docs.Plugin.Configs
{
    public class DocDescriptionConfig : IPluginConfig
    {
        private readonly List<Type> _descriptionTypes = new List<Type>();

        public DocDescriptionConfig()
        {
            AddDocDescription<XmlActionComments>();
            AddDocDescription<XmlParamComments>();
            AddDocDescription<XmlResultComments>();

            DescriptionTypes = _descriptionTypes.AsReadOnly();
        }

        public void AddDocDescription<T>()
            where T : IDocDescription
        {
            if (_descriptionTypes.Contains(typeof(T))) return;
            
            _descriptionTypes.Add(typeof(T));
        }
        
        public IReadOnlyCollection<Type> DescriptionTypes { get; }
    }
}