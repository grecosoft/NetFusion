// using System;
// using System.IO;
// using System.Text.Json;
// using FluentAssertions;
// using NetFusion.Web.Rest.Docs.Plugin.Configs;
// using NetFusion.Web.Rest.Docs.Xml.Descriptions;
// using Xunit;
//
// namespace NetFusion.Web.UnitTests.Rest.DocGeneration
// {
//     /// <summary>
//     /// Tests that validation the correct configuration of the plugin.
//     /// </summary>
//     public class DocConfigTests
//     {
//         /// <summary>
//         /// By default, the location containing any additional files used to
//         /// add addition documentations to the returned document model are
//         /// contained within the application host's base directory.  This
//         /// directory, for example, can contain VS C# Code comment XML files.
//         /// </summary>
//         [Fact]
//         public void ByDefaultDescriptionFiles_LocatedInBaseDirectory()
//         {
//             var config = new RestDocConfig();
//             config.DescriptionDirectory.Should().Be(AppContext.BaseDirectory);
//         }
//
//         /// <summary>
//         /// When the plugin is bootstrapped, the location of description
//         /// files can be specified.
//         /// </summary>
//         [Fact]
//         public void BootstrapCan_Specify_FileDescriptionDirectory()
//         {
//             var config = new RestDocConfig();
//             config.SetDescriptionDirectory(Path.Combine(AppContext.BaseDirectory, "comment-files"));
//             config.DescriptionDirectory.Should().Be(Path.Combine(AppContext.BaseDirectory, "comment-files"));
//         }
//
//         /// <summary>
//         /// A middleware component exposes an endpoint called by clients to
//         /// obtain documentation for a given WebApi controller method.
//         /// </summary>
//         [Fact]
//         public void DocumentationEndpoint_HasDefaultValue()
//         {
//             var config = new RestDocConfig();
//             config.EndpointUrl.Should().Be("/api/net-fusion/rest");
//         }
//
//         /// <summary>
//         /// During the bootstrap process, the default documentation endpoint
//         /// can be overriden.
//         /// </summary>
//         [Fact]
//         public void BootstrapCan_Specify_DocumentationEndPoint()
//         {
//             var config = new RestDocConfig();
//             config.UseEndpoint("api/use/this");
//             config.EndpointUrl.Should().Be("api/use/this");
//         }
//
//         /// <summary>
//         /// By default, Camel Case serialization settings are used.
//         /// </summary>
//         [Fact]
//         public void DefaultSerializationSettingsSpecified()
//         {
//             var config = new RestDocConfig();
//             config.SerializerOptions.Should().NotBeNull();
//             config.SerializerOptions.PropertyNamingPolicy.Should().Be(JsonNamingPolicy.CamelCase);
//         }
//
//         /// <summary>
//         /// When bootstrapping, the default serialization settings can
//         /// be overriden with custom settings.
//         /// </summary>
//         [Fact]
//         public void SerializationSetting_CanBeSpecified()
//         {
//             var config = new RestDocConfig();
//             config.UseSerializationOptions(new JsonSerializerOptions {IgnoreNullValues = true});
//             config.SerializerOptions.IgnoreNullValues.Should().BeTrue();
//         }
//
//         /// <summary>
//         /// As the Action Document Model is being created, it will invoke
//         /// configured classes deriving from IDocDescription.  These classes
//         /// are responsible for adding details to the created document model.
//         /// By default, a set of description classes are registered that will
//         /// add comments containing within .NET code commit files.
//         /// </summary>
//         [Fact]
//         public void DescriptionImplementations_SpecifiedByDefault()
//         {
//             var config = new RestDocConfig();
//             config.DescriptionTypes.Should().NotBeNullOrEmpty();
//         }
//
//         /// <summary>
//         /// The list of registered descriptions can be changed during
//         /// bootstrapping of the host.
//         /// </summary>
//         [Fact]
//         public void DescriptionImplementations_Specified()
//         {
//             var config = new RestDocConfig();
//             config.ClearDescriptions();
//             config.AddDocDescription<XmlActionComments>();
//             config.AddDocDescription<XmlParameterComments>();
//             config.DescriptionTypes.Should().HaveCount(2);
//         }
//     }
// }