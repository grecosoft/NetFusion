using System;
using FluentAssertions;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Resources.Hal;
using Xunit;

namespace WebTests.Rest.Resources
{
    /// <summary>
    /// These unit-tests validate that the correct embedded name is used.
    /// </summary>
    public class EmbeddedTests
    {
        [Fact]
        public void NameIdentifiesEmbeddedResource()
        {
            var customerRes = new Customer
            {
                FirstName = "Doug",
                LastName = "Bowan"
            }.AsResource();
            
            customerRes.EmbedModel(new Comment
            {
                Value = "The food was so-so.",
                DateSubmitted = DateTime.UtcNow
            }.AsResource(), "master-eater");

            customerRes.Embedded.Should().HaveCount(1);
            customerRes.Embedded.Should().ContainKey("master-eater");
        }

        [Fact]
        public void ExceptionIfNameNotExplicitlySpecifiedOrByAttribute()
        {
            var customerRes = new Customer
            {
                FirstName = "Chris",
                LastName = "Smith"
            }.AsResource();

            customerRes.EmbedModel(new Note());
            customerRes.Embedded.Should().ContainKey(typeof(Note).FullName);
        }

        [Fact]
        public void AttributeSpecifiedNameIdentifiedForEmbeddedResource()
        {
            var customerRes = new Customer
            {
                FirstName = "Mark",
                LastName = "Twain"
            }.AsResource();
            
            customerRes.EmbedResource(new Rating
            {
                Value = 98,
                DateSubmitted = DateTime.UtcNow
            }.AsResource());

            customerRes.HasEmbedded("customer-rating").Should().BeTrue();
        }

        [Fact]
        public void AttributeSpecifiedNameIdentifiedForEmbeddedModel()
        {
            var customerRes = new Customer
            {
                FirstName = "Mark",
                LastName = "Twain"
            }.AsResource();
            
            customerRes.EmbedModel(new Rating
            {
                Value = 98,
                DateSubmitted = DateTime.UtcNow
            });

            customerRes.HasEmbedded("customer-rating").Should().BeTrue();
        }
        
        [Fact]
        public void ExplicitNameOverridesAttributeSpecifiedName()
        {
            var customerRes = new Customer
            {
                FirstName = "Mark",
                LastName = "Twain"
            }.AsResource();
            
            customerRes.EmbedModel(new Rating
            {
                Value = 98,
                DateSubmitted = DateTime.UtcNow
            }, "user-rating");

            customerRes.HasEmbedded("user-rating").Should().BeTrue();
        }

        [Fact]
        public void AttributeSpecifiedNameIdentifiedForEmbeddedResourceCollection()
        {
            var customerRes = new Customer
            {
                FirstName = "Mark",
                LastName = "Twain"
            }.AsResource();

            var ratings = new[]
            {
                new Rating { Value = 90, DateSubmitted = DateTime.UtcNow },
                new Rating { Value = 78, DateSubmitted = DateTime.UtcNow }
            }.AsResources();
            
            customerRes.EmbedResources(ratings);
            customerRes.HasEmbedded("customer-rating").Should().BeTrue();
        }

        [Fact]
        public void AttributeSpecifiedNameIdentifiedForEmbeddedModelCollection()
        {
            var customerRes = new Customer
            {
                FirstName = "Mark",
                LastName = "Twain"
            }.AsResource();

            var ratings = new[]
            {
                new Rating { Value = 90, DateSubmitted = DateTime.UtcNow },
                new Rating { Value = 78, DateSubmitted = DateTime.UtcNow }
            };
            
            customerRes.EmbedModels(ratings);
            customerRes.HasEmbedded("customer-rating").Should().BeTrue();
        }

        public class Customer
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
        
        public class Comment
        {
            public string Value { get; set; }
            public DateTime DateSubmitted { get; set; }
        }

        public class Note
        {

        }

        [ExposedName("customer-rating")]
        public class Rating
        {
            public int Value { get; set; }
            public DateTime DateSubmitted { get; set; }
        }
    }
}