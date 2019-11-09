using NetFusion.Base.Plugins;

namespace Demo.Core
{
    public interface IValueCalculator : IKnownPluginType
    {
        int Sequence { get; }
        int GetValue(int[] values);
    }
}
