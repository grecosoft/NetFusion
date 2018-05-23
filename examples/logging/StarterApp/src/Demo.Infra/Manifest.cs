using NetFusion.Bootstrap.Manifests;

namespace Demo.Infra
{
    public class Manifest : PluginManifestBase,
        ICorePluginManifest
  {
    public string PluginId => "C7D17C4B-2604-4310-97B7-7208AD8027F3";
    public string Name => "Infrastructure Component";
    public string Description => "The plugin containing application infrastructure.";
  }
}
