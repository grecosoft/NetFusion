using NetFusion.Messaging.Types;

namespace Service.Domain.Commands
{
    public class GeneratedDataResponse : Message
    {
        public string UniqueIdValue { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}