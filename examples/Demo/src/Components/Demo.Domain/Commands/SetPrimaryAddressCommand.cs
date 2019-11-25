using System;
using NetFusion.Messaging.Types;

namespace Demo.Domain.Commands
{
    public class SetPrimaryAddressCommand : Command
    {
        public Guid CustomerId { get; }
        public Guid AddressId { get; }
        
        public SetPrimaryAddressCommand(Guid customerId, Guid addressId)
        {
            CustomerId = customerId;
            AddressId = addressId;
        }
    }
}