using System;
using NetFusion.Messaging.Types;

namespace Demo.Domain.Commands
{
    public class UpdateCarFaxReport : Command
    {
        public string Vin { get; set; }
        public DateTime DateOfService { get; set; }
        public string Service { get; set; }
        public int NumberOfOwners { get; set; }
        public bool IsTotalLoss { get; set; }
        public int Miles { get; set; }
    }
}