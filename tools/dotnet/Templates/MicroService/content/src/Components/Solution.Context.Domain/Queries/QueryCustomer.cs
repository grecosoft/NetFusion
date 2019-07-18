using System;
using NetFusion.Messaging.Types;
using Solution.Context.Domain.Entities;

namespace Solution.Context.Domain.Queries
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