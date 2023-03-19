// using System.Collections.Generic;
// using System.Linq;
// using FluentAssertions;
// using NetFusion.Web.Rest.Docs.Core.Descriptions;
// using NetFusion.Web.Rest.Docs.Plugin;
// using NetFusion.Web.Rest.Docs.Plugin.Configs;
// using NetFusion.Web.Rest.Docs.Plugin.Modules;
// using NetFusion.Core.TestFixtures.Container;
// using NetFusion.Web.UnitTests.Rest.DocGeneration.Setup;
// using Xunit;
//
// namespace NetFusion.Web.UnitTests.Rest.DocGeneration
// {
//     /// <summary>
//     /// Contains test specific to the proper initialization and execution of the documentation
//     /// plugin module.  The module configures documentation related components that are used when
//     /// building a document model for a specific WebApi action method.
//     /// </summary>
//     public class DocModuleTests
//     {
//         /// <summary>
//         /// So other services can obtain a reference to the configuration,
//         /// the configuration is exposed as a module property.
//         /// </summary>
//         [Fact]
//         public void Module_StoresReference_ToPluginConfiguration()
//         {
//             ContainerFixture.Test(fixture =>
//             {
//                 fixture.Arrange.Container(PluginSetup.WithDefaults)
//                     .Act.OnApplication(a => a.Start())
//                     .Assert.PluginModule<DocModule>(m =>
//                     {
//                         m.RestDocConfig.Should().NotBeNull();
//                     }).Service<IDocModule>(s =>
//                     {
//                         s.RestDocConfig.Should().NotBeNull();
//                     });
//             });
//         }
//
//         /// <summary>
//         /// The IDocDescription classes, listed by the RestDocConfig class, are
//         /// invoked to add additional details to the document model.  These classes
//         /// are registered as services so they can have other services injected.
//         /// </summary>
//         [Fact]
//         public void Module_RegistersDescriptions_AsServices()
//         {
//             ContainerFixture.Test(fixture =>
//             {
//                 var docGenConfig = new RestDocConfig();
//                 
//                 fixture.Arrange.Container(PluginSetup.WithDefaults)
//                     .Act.OnApplication(a => a.Start())
//                     .Assert.Service<IEnumerable<IDocDescription>>(docDescItems =>
//                     {
//                         docDescItems = docDescItems?.ToArray();
//                         
//                         docDescItems.Should().NotBeNull();
//                         docDescItems.Should().NotBeEmpty();
//                         docDescItems.Should().HaveCount(docGenConfig.DescriptionTypes.Count);
//                     });
//             });
//         }
//
//         /// <summary>
//         /// Some items such as Embedded Resources and Links don't have corresponding
//         /// documentation that can easily be specified within the .NET code generated
//         /// XML files.  These additional comments are stored within a JSON file.
//         /// XML files.  These additional comments are stored within a JSON file.
//         /// </summary>
//         [Fact]
//         public void AdditionalComment_AreLoaded_FromJsonFile()
//         {
//             ContainerFixture.Test(fixture =>
//             {
//                 fixture.Arrange.Container(PluginSetup.WithDefaults)
//                     .Act.OnApplication(a => a.Start())
//                     .Assert.Service<IDocModule>(s =>
//                     {
//                         s.HalComments.Should().NotBeNull();
//                         s.HalComments.EmbeddedComments.Should().NotBeEmpty();
//                         s.HalComments.RelationComments.Should().NotBeEmpty();
//                     });
//             });
//         }
//     }
// }