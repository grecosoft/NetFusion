using NetFusion.Base.Plugins;

namespace Demo.Infra
{
    public interface IValueCalculator : IKnownPluginType
    {
        int Sequence { get; }
        int GetValue(int[] values);
    }
}