using System;
using FluentAssertions;
using NetFusion.Common.Extensions;
using Xunit;

namespace CommonTests.Extensions
{
    public class ObjectTests
    {
        [Fact]
        public void CanSerializeObject_To_IndentedJson()
        {
            var customer = new Customer
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Alex",
                LastName = "Green"
            };

            customer.ToIndentedJson().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void CanSerializeObject_to_Json()
        {
            var customer = new Customer
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Alex",
                LastName = "Green"
            };

            customer.ToJson().Should().NotBeNullOrEmpty();
        }

        private class Customer
        {
            public string Id { get; init; }
            public string FirstName { get; init; }
            public string LastName { get; init; }
        }
    }
}
