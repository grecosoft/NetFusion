namespace NetFusion.Core.Bootstrap.Plugins;

/// <summary>
/// Marker interface used to identify interfaces implemented by modules providing access to information
/// it manages. All, modules implementing an interface derived from this type will be added to the
/// dependency-injection container as singletons of the derived interface type.
/// </summary>
public interface IPluginModuleService
{
}