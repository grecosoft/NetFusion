namespace Service.Domain.Entities
{
    using NetFusion.Settings;

    [ConfigurationSection("calculations:inputs")]
    public class CalculationSettings : IAppSettings
    {
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int[] InvalidValues { get; set; }
        public CalcValidator[] Validators { get; set; }
    }
}