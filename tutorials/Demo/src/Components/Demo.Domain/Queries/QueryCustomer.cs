using System;
using NetFusion.Messaging.Types;
using Demo.Domain.Entities;

namespace Demo.Domain.Queries
{
    public class QueryCustomer : Query<CustomerInfo>
    {
        public Guid Id { get; }
        public bool IncludedSuggestions { get; set; }

        public QueryCustomer(Guid id)
        {
            Id = id;
        }
    }
}