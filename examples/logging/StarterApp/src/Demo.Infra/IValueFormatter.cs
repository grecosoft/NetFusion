using NetFusion.Base.Plugins;

namespace Demo.Infra
{
    public interface IValueFormatter : IKnownPluginType
    {
        string FormatValues(object[] values);
    }
}