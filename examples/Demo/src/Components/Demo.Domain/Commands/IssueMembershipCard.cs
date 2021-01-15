using System;
using NetFusion.Messaging.Types;

namespace Demo.Domain.Commands
{
    public class IssueMembershipCard : Command
    {
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}