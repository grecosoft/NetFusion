using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using NetFusion.Common.Extensions.Reflection;
using Xunit;

namespace CommonTests.Extensions.Reflection
{
    public class MemberTests
    {
        [Fact]
        public void CanDetermine_IfNullableType()
        {
            typeof(TestClass).GetProperty("Sequence").IsNullable().Should().BeTrue();
            typeof(TestClass).GetProperty("Name").IsNullable().Should().BeTrue();
            typeof(TestClass).GetProperty("DateRequested").IsNullable().Should().BeTrue();
            typeof(TestClass).GetProperty("Total").IsNullable().Should().BeFalse();
        }

        [Fact]
        public void CanDetermine_IfMemberMarkedRequired()
        {
            typeof(TestClass).GetProperty("Name").IsMarkedRequired().Should().BeTrue();
        }

        [Fact]
        public void CanDetermine_IfEnumerableType()
        {
            typeof(TestClass).GetProperty("Name").IsEnumerable().Should().BeFalse();
            typeof(TestClass).GetProperty("MissedSequences").IsEnumerable().Should().BeTrue();
            typeof(TestClass).GetProperty("Total").IsEnumerable().Should().BeFalse();
            typeof(TestClass).GetProperty("Rates").IsEnumerable().Should().BeTrue();
            typeof(TestClass).GetProperty("Ages").IsEnumerable().Should().BeTrue();
        }

        [Fact]
        public void CanDetermine_EnumerableOfType()
        {
            typeof(TestClass).GetProperty("Name").GetEnumerableType().Should().BeNull();
            typeof(TestClass).GetProperty("MissedSequences").GetEnumerableType().Should().Be(typeof(int));
            typeof(TestClass).GetProperty("Total").GetEnumerableType().Should().BeNull();
            typeof(TestClass).GetProperty("Rates").GetEnumerableType().Should().Be(typeof(decimal));
            typeof(TestClass).GetProperty("Ages").GetEnumerableType().Should().Be(typeof(int));
        }

        private class TestClass
        {
            public int? Sequence { get; set; }
            
            [Required]
            public string Name { get; set; }
            public DateTime? DateRequested { get; set; }
            public int Total { get; set; }

            public List<int> MissedSequences { get; set; }
            public IReadOnlyCollection<decimal> Rates { get; set; }
            public int[] Ages { get; set; }
        }
    }
}