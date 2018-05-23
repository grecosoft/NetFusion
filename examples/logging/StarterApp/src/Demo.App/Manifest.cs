using NetFusion.Bootstrap.Manifests;

namespace Demo.Infra
{
  public class Manifest : PluginManifestBase,
    IAppComponentPluginManifest
  {
    public string PluginId => "ABF99168-CFD0-4B7E-9704-4094C26FD019";
    public string Name => "Application Service Component";
    public string Description => "The plugin containing application services.";
  }
}
