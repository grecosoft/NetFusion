using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NetFusion.Rest.Docs;
using NetFusion.Rest.Docs.Core.Descriptions;
using NetFusion.Rest.Docs.Json.Descriptions;
using NetFusion.Rest.Docs.Models;
using NetFusion.Rest.Server.Linking;
using NetFusion.Test.Container;
using WebTests.Rest.DocGeneration.Server;
using WebTests.Rest.DocGeneration.Setup;
using Xunit;

namespace WebTests.Rest.DocGeneration
{
    /// <summary>
    /// These are tests for documentation comments that are read from an external JSON file.
    /// This is the case for Embedded resources and resource link relations for which no
    /// existing attribute exists to represent these HAL based comments.
    /// </summary>
    public class JsonDescriptionTests
    {
        /// <summary>
        /// The comments to associate with an embedded resource, returned from an WebApi
        /// controller, are stored within a JSON file.  If an entry exists based on the
        /// embedded name, parent resource name, and child resource names, the associated
        /// comment will be used.  
        /// </summary>
        [Fact]
        public void EmbeddedComment_FoundForExactMatch()
        {
            // Arrange:
            var embeddedDoc = new ApiEmbeddedDoc { EmbeddedName = "embedded-child" };
            var attrib = new EmbeddedResourceAttribute(typeof(RootResponseModel), typeof(EmbeddedChildModel), "embedded-child");
            
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(PluginSetup.WithDefaults)
                    .Act.OnApplication(a => a.Start())
                    .Assert.Service<IEnumerable<IDocDescription>>(descriptions =>
                    {
                        var jsonDesc = descriptions.OfType<JsonHalEmbeddedComments>().FirstOrDefault();
                        jsonDesc.Should().NotBeNull();
                        
                       jsonDesc?.Describe(embeddedDoc, attrib);
                       embeddedDoc.Description.Should().Be("exact_match_comment");
                    });
            });
        }
        
        /// <summary>
        /// If an exact match is not found as tested in the prior unit-test, a search is completed
        /// based on just the embedded name.  Of found, the associated comments are used. 
        /// </summary>
        [Fact]
        public void EmbeddedComment_FoundForPartialMatch()
        {
            // Arrange:
            var embeddedDoc = new ApiEmbeddedDoc { EmbeddedName = "embedded-child-partial" };
            var attrib = new EmbeddedResourceAttribute(typeof(RootResponseModel), typeof(EmbeddedChildModel), "embedded-child-partial");
            
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(PluginSetup.WithDefaults)
                    .Act.OnApplication(a => a.Start())
                    .Assert.Service<IEnumerable<IDocDescription>>(descriptions =>
                    {
                        var jsonDesc = descriptions.OfType<JsonHalEmbeddedComments>().FirstOrDefault();
                        jsonDesc.Should().NotBeNull();
                        
                        jsonDesc?.Describe(embeddedDoc, attrib);
                        embeddedDoc.Description.Should().Be("partial_match_comment");
                    });
            });
        }

        /// <summary>
        /// When a relation document is built, the associated comment is determined based on the
        /// resource name containing the link and the name of the link.  
        /// </summary>
        [Fact]
        public void RelationComment_FoundForExactMatch()
        {
            // Arrange:
            var resourceDoc = new ApiResourceDoc { ResourceName = "resource-with-relation" };
            var relationDoc = new ApiRelationDoc { Name = "current-child" };
            var link = new ResourceLink { Name = "current-child" };
            
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(PluginSetup.WithDefaults)
                    .Act.OnApplication(a => a.Start())
                    .Assert.Service<IEnumerable<IDocDescription>>(descriptions =>
                    {
                        var jsonDesc = descriptions.OfType<JsonHalRelationComments>().FirstOrDefault();
                        jsonDesc.Should().NotBeNull();
                        
                        jsonDesc?.Describe(resourceDoc, relationDoc, link);
                        relationDoc.Description.Should().Be("exact_match_comment");
                    });
            });
        }

        /// <summary>
        /// If an exact relation is not found as described in the prior unit test, then a
        /// search based on only the relation name is performed.
        /// </summary>
        [Fact]
        public void RelationComment_FoundForPartialMatch()
        {
            // Arrange:
            var resourceDoc = new ApiResourceDoc { ResourceName = "resource-with-relation" };
            var relationDoc = new ApiRelationDoc { Name = "self" };
            var link = new ResourceLink { Name = "self" };
            
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(PluginSetup.WithDefaults)
                    .Act.OnApplication(a => a.Start())
                    .Assert.Service<IEnumerable<IDocDescription>>(descriptions =>
                    {
                        var jsonDesc = descriptions.OfType<JsonHalRelationComments>().FirstOrDefault();
                        jsonDesc.Should().NotBeNull();
                        
                        jsonDesc?.Describe(resourceDoc, relationDoc, link);
                        relationDoc.Description.Should().Be("partial_match_comment");
                    });
            });
        }
    }
}