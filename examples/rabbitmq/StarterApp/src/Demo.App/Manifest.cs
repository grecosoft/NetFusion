using NetFusion.Bootstrap.Manifests;

namespace Demo.App
{
  public class Manifest : PluginManifestBase,
    IAppComponentPluginManifest
  {
    public string PluginId => "ABF99168-CFD0-4B7E-9704-4094C26FD019";
    public string Name => "Core Infrastructure Services";
    public string Description => "The plugin containing core services.";
  }
}