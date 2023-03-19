// using FluentAssertions;
// using NetFusion.Web.UnitTests.Rest.DocGeneration.Server;
// using NetFusion.Web.UnitTests.Rest.DocGeneration.Setup;
// using Xunit;
//
// namespace NetFusion.Web.UnitTests.Rest.DocGeneration
// {
//     public class XmlCommentTests
//     {
//         /// <summary>
//         /// Xml comments are stored in files based on the assembly for which they where generated.
//         /// </summary>
//         [Fact]
//         public void CanObtainXmlNavigator_ForAssemblyNode()
//         {
//             var xmlPathNav = XmlCommentsSetup.XmlService.GetXmlCommentsForTypesAssembly(typeof(TestResource));
//             xmlPathNav.Should().NotBeNull();
//         }
//
//         /// <summary>
//         /// The top most node within the comments are for types defined within the assembly. 
//         /// </summary>
//         [Fact]
//         public void CanObtainXmlNavigator_ForTypeNode()
//         {
//             var xmlPathNav = XmlCommentsSetup.XmlService.GetTypeNode(typeof(TestResource));
//             xmlPathNav.Should().NotBeNull();
//         }
//
//         /// <summary>
//         /// If only the comment of a type is needed, this can be directly requested.
//         /// </summary>
//         [Fact]
//         public void CanObtainComments_ForType()
//         {
//             var comments = XmlCommentsSetup.XmlService.GetTypeComments(typeof(TestResource));
//             comments.Should().Be("Class comment for test-resource.");
//         }
//
//         /// <summary>
//         /// The XML node for a given type's method can be retrieved.
//         /// </summary>
//         [Fact]
//         public void CanObtainXmlNavigator_ForMethodNode()
//         {
//             var method = typeof(TestResource).GetMethod("TestMethod");
//             var xmlPathNav = XmlCommentsSetup.XmlService.GetMethodNode(method);
//             xmlPathNav.Should().NotBeNull();
//         }
//
//         /// <summary>
//         /// The comments for a type's method can be directory requested.
//         /// </summary>
//         [Fact]
//         public void CanObtainComments_ForMethod()
//         {
//             var method = typeof(TestResource).GetMethod("TestMethod");
//             var comments = XmlCommentsSetup.XmlService.GetMethodComments(method);
//             comments.Should().Be("Comment associated with a method.");
//         }
//         
//         /// <summary>
//         /// Comments for a type's property can be directly requested.
//         /// </summary>
//         [Fact]
//         public void CanObtainComments_ForTypeMember()
//         {
//             var property = typeof(TestResource).GetProperty("FirstValue");
//             var comments = XmlCommentsSetup.XmlService.GetTypeMemberComments(property);
//             comments.Should().Be("Example string property comment.");
//         }
//
//         /// <summary>
//         /// Given a type's method Xml comment node, the comments for a parameter can be requested.
//         /// </summary>
//         [Fact]
//         public void CanObtainComments_ForMethodParameter()
//         {
//             var method = typeof(TestResource).GetMethod("TestMethodWithParam");
//             var xmlPathNav = XmlCommentsSetup.XmlService.GetMethodNode(method);
//             var comments = XmlCommentsSetup.XmlService.GetMethodParamComment(xmlPathNav, "itemId");
//             
//             comments.Should().Be("Comment associated with a parameter.");
//         }
//     }
// }