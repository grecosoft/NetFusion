using NetFusion.Messaging.Types;

namespace Demo.Subscriber.Commands
{
    [MessageNamespace("demo.examples.cal.range")]
    public class CalculateRange : Command<ValueRange>
    {
        public int[] Values { get; set; }
    }
}