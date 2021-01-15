using NetFusion.Messaging.Types;

namespace Demo.Domain.Commands
{
    public class CarFaxUpdateResult : Message
    {
        public string ReportId { get; set; }
        public string ReportUrl { get; set; }
        public string ResultStatus { get; set; }
    }
}