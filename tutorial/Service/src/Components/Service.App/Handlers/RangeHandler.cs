using System;
using System.Linq;
using System.Threading.Tasks;
using NetFusion.Messaging;
using Service.Domain.Commands;

namespace Service.App.Handlers
{
    public class RangeHandler : IMessageConsumer
    {
        [InProcessHandler]
        public async Task<ValueRange> DetermineRange(CalculateRange command)
        {
            if (command.Values.Length == 0)
            {
                throw new InvalidOperationException("The range can't be calculated.");
            }
            
            var range = new ValueRange(command.Values.Min(), command.Values.Max());
            
            range.Message = command.MessageTemplate
                .Replace("{min}", range.Min.ToString())
                .Replace("{max}", range.Max.ToString());

            await Task.Delay(TimeSpan.FromSeconds(5));
            return range;
        }
    }
}