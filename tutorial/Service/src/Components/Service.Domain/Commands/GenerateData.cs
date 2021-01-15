using NetFusion.Messaging.Types;

namespace Service.Domain.Commands
{
    public class GenerateData : Command
    {
        public bool IncludeUniqueId { get; set; }
    }
}