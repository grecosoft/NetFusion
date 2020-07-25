using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using NetFusion.Rest.Client;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Server.Hal;
using Xunit;

namespace WebTests.Rest.Resources
{
    
    /// <summary>
    /// These tests validate the property serialization of resources using the new System.Text.Json serializer.
    /// Resources are returned from WebApi controller methods.  These tests validate that resource wrapped
    /// models are correctly returned to the client.  Calling clients invoking actions on resources only
    /// submit back the model and not the resource.  
    /// </summary>
    public class SerializationTests
    {
        private static JsonSerializerOptions SerializerOptions => new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        /// <summary>
        /// A WebApi can return a single resource wrapping a model and containing
        /// links pertaining to the model.
        /// </summary>
        [Fact]
        public void SingleResource()
        {
            var resource = TestModel.AsResource();
            resource.Links = TestLinks;

            var json = JsonSerializer.Serialize(resource, SerializerOptions);
            
            // Deserialize json back into object to determine JSON contained 
            // expected serialized data:
            var result = JsonSerializer.Deserialize<HalResource<Sensor>>(json, SerializerOptions);

            result.Should().NotBeNull();
            result.Model.Should().BeEquivalentTo(TestModel);
            result.Links.Should().NotBeNull();
            result.Links.Should().BeEquivalentTo(TestLinks);
        }

        /// <summary>
        /// A WebApi can return a single resource containing a child related embedded resource.
        /// </summary>
        [Fact]
        public void SingleEmbeddedResource()
        {
            var resource = TestModel.AsResource();
            resource.Links = TestLinks;

            var stats = new Stats
            {
                DateStarted = DateTime.Now,
                NumberReads = 499
            };

            var embeddedResource = stats.AsResource();
            embeddedResource.Links = new Dictionary<string, Link>
            {
                { "reset", new Link { Name = "reset", Href = "http://sensor/stats/reset"}}
            };
            
            resource.EmbedModel(embeddedResource, "stats");
            
            var json = JsonSerializer.Serialize(resource, SerializerOptions);
            
            // Deserialize json back into object to determine JSON contained 
            // expected serialized data:
            var result = JsonSerializer.Deserialize<HalResource<Sensor>>(json, SerializerOptions);
            result.Embedded.Should().NotBeNull();
            result.Embedded.Should().ContainKey("stats");
            
            // Validate the embedded resource:
            var embeddedResult = result.GetEmbeddedResource<Stats>("stats");
            embeddedResult.Should().NotBeNull();
            embeddedResult.Model.Should().BeEquivalentTo(stats);
            embeddedResult.Links.Should().BeEquivalentTo(embeddedResource.Links);
        }

        /// <summary>
        /// A WebApi can return a resource containing an embedded collection of
        /// related resources.
        /// </summary>
        [Fact]
        public void CollectionOfEmbeddedResources()
        {
            var resource = TestModel.AsResource();
            resource.Links = TestLinks;

            var stats = new[]
            {
                new Stats { DateStarted = DateTime.Now, NumberReads = 444 },
                new Stats { DateStarted = DateTime.Now, NumberReads = 555 }
            };

            var embeddedResources = stats.AsResources();
            
            // Add some links to embedded resource:
            foreach (var embeddedResource in embeddedResources)
            {
                embeddedResource.Links = new Dictionary<string, Link>
                {
                    { "reset", new Link { Name = "reset", Href = "http://sensor/stats/reset"}}
                };
            }
            
            resource.EmbedModel(embeddedResources, "stats");
            
            var json = JsonSerializer.Serialize(resource, SerializerOptions);
            
            // Deserialize json back into object to determine JSON contained 
            // expected serialized data:
            var result = JsonSerializer.Deserialize<HalResource<Sensor>>(json, SerializerOptions);
            result.Embedded.Should().NotBeNull();
            result.Embedded.Should().ContainKey("stats");
            
            // Validate the embedded resource:
            var embeddedResults = result.GetEmbeddedResources<Stats>("stats").ToArray();
            embeddedResults.Should().HaveCount(2);
          
            embeddedResults[0].Model.Should().BeEquivalentTo(embeddedResources[0].Model);
            embeddedResults[0].Links.Should().BeEquivalentTo(embeddedResources[0].Links);
            embeddedResults[1].Model.Should().BeEquivalentTo(embeddedResources[1].Model);
            embeddedResults[1].Links.Should().BeEquivalentTo(embeddedResources[1].Links);
        }

        /// <summary>
        /// The server can also embedded a model if there is no resource related information.
        /// </summary>
        [Fact]
        public void SingleEmbeddedModel()
        {
            var resource = TestModel.AsResource();
            resource.Links = TestLinks;

            var stats = new Stats
            {
                DateStarted = DateTime.Now,
                NumberReads = 499
            };
            
            resource.EmbedModel(stats, "model");
            
            var json = JsonSerializer.Serialize(resource, SerializerOptions);
            
            // Deserialize json back into object to determine JSON contained 
            // expected serialized data:
            var result = JsonSerializer.Deserialize<HalResource<Sensor>>(json, SerializerOptions);
            
            // Validate the embedded resource:
            var embeddedModel = result.GetEmbeddedModel<Stats>("model");
            embeddedModel.Should().BeEquivalentTo(stats);
        }

        /// <summary>
        /// The server can also embedded a collection of models if there is no resource related information.
        /// </summary>
        [Fact]
        public void CollectionOfEmbeddedModels()
        {
            var resource = TestModel.AsResource();
            resource.Links = TestLinks;

            var stats = new[]
            {
                new Stats { DateStarted = DateTime.Now, NumberReads = 294 },
                new Stats { DateStarted = DateTime.Now, NumberReads = 555 }
            };
            
            resource.EmbedModel(stats, "models");
            
            var json = JsonSerializer.Serialize(resource, SerializerOptions);
            
            // Deserialize json back into object to determine JSON contained 
            // expected serialized data:
            var result = JsonSerializer.Deserialize<HalResource<Sensor>>(json, SerializerOptions);
            
            // Validate the embedded resource:
            var embeddedModels = result.GetEmbeddedModels<Stats>("models").ToArray();
            embeddedModels.Should().HaveCount(2);
            embeddedModels[0].Should().BeEquivalentTo(stats[0]);
            embeddedModels[1].Should().BeEquivalentTo(stats[1]);
        }
        
        private static Sensor TestModel => new Sensor
        {
            SensorId = "S435345",
            Name = "Sensor-One",
            Values = new []{ 10, 40, 60 }
        };

        private static IDictionary<string, Link> TestLinks => new Dictionary<string, Link>
        {
            { "status", new Link {Name = "status", Href = "http://sensor/status"} },
            { "version", new Link {Name = "version", Href = "http://sensor/version"} }
        };
    }


    public class Sensor
    {
        public string SensorId { get; set; }
        public string Name { get; set; }
        public int[] Values { get; set; }
    }

    public class Stats
    {
        public DateTime DateStarted { get; set; }
        public int NumberReads { get; set; }
    }
}