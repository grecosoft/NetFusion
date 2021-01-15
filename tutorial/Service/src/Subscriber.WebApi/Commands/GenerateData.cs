using NetFusion.Messaging.Types;

namespace Subscriber.WebApi.Commands
{
    public class GenerateData : Command<GeneratedDataResponse>
    {
        public bool IncludeUniqueId { get; set; }
    }
}