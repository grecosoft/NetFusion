using FluentAssertions;
using NetFusion.Common.Extensions.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CommonTests.Extensions.Collections
{
    public class EnumerableTests
    {

        [Fact(DisplayName = nameof(GivenLookup_ReteriveAllValues))]
        public void GivenLookup_ReteriveAllValues()
        {
            var data = new[] {
                new TestData { EventType = "NewCustomer", HandlerName = "CreateContact" },
                new TestData { EventType = "NewCustomer", HandlerName = "SendEmail" },
                new TestData { EventType = "NewOrder", HandlerName = "CheckInventory" }
            };

            var lookup = data.ToLookup(d => d.EventType);
            var values = lookup.Values().ToArray();

            values.Should().NotBeNull();
            values.Should().HaveCount(3);
        }

        [Fact(DisplayName = nameof(GivenEnumerable_CheckIfEmpty))]
        public void GivenEnumerable_CheckIfEmpty()
        {
            var list = new List<int> { 5 };
            list.Empty().Should().BeFalse();
            list.Clear();
            list.Empty().Should().BeTrue();
        }

        [Fact(DisplayName = nameof(GivenEnumerable_FindForDuplicateElementPropertyValues))]
        public void GivenEnumerable_FindForDuplicateElementPropertyValues()
        {
            var data = new[] {
                new TestData { EventType = "NewCustomer", HandlerName = "CreateContact" },
                new TestData { EventType = "NewCustomer", HandlerName = "SendEmail" },
                new TestData { EventType = "NewOrder", HandlerName = "CheckInventory" }
            };

            var duplicates = data.WhereDuplicated(d => d.EventType).ToArray();
            duplicates.Should().HaveCount(1);
            duplicates.First().Should().Be("NewCustomer");
            data.WhereDuplicated(d => d.HandlerName).Should().BeEmpty();
        }

        [Fact (DisplayName = nameof(GivenEnumerable_OrderItemsByType))]
        public void GivenEnumerable_OrderItemsByType()
        {
            var instances = new IItem[] {
                new ItemOne(),
                new ItemTwo(),
                new ItemThree()
            };

            var orderedInstances = instances.OrderByMatchingType(
                new[] { typeof(ItemThree), typeof(ItemOne), typeof(ItemTwo) }).ToArray();

            orderedInstances[0].Should().Be(instances[2]);
            orderedInstances[1].Should().Be(instances[0]);
            orderedInstances[2].Should().Be(instances[1]);
        }

        private interface IItem
        {

        }

        public class ItemOne : IItem { }
        public class ItemTwo : IItem { }
        public class ItemThree : IItem { }

        private class TestData
        {
            public string EventType { get; set; }
            public string HandlerName { get; set; }
        }

    }
}
