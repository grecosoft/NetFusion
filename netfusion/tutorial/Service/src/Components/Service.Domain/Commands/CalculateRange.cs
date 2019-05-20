using NetFusion.Messaging.Types;

namespace Service.Domain.Commands
{
    public class CalculateRange : Command<Range>
    {
        public string MessageTemplate { get; }
        public int[] Values { get; }

        public CalculateRange(string messageTemplate, int[] values)
        {
            MessageTemplate = messageTemplate;
            Values = values;
        }
    }

    public class Range
    {
        public int Min { get; }
        public int Max { get; }
        public string Message { get; set; }

        public Range(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }
}