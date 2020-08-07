// using System.Linq;
// using FluentAssertions;
// using WebTests.Rest.DocGeneration.Server;
// using WebTests.Rest.DocGeneration.Setup;
// using Xunit;
//
// namespace WebTests.Rest.DocGeneration
// {
//     public class XmlTypeCommentTests
//     {
//         /// <summary>
//         /// The description of a resource is set based on XML comment for that type.
//         /// </summary>
//         [Fact]
//         public void TypeComments_AddedToDocModel()
//         {
//             var resourceDoc = XmlCommentsSetup.TypeService.GetResourceDoc(typeof(TestResource));
//             resourceDoc.Should().NotBeNull();
//             resourceDoc.Description.Should().Be("Class comment for test-resource.");
//         }
//
//         /// <summary>
//         /// For each resource property, the associated comments are found in the XML comments
//         /// and specified on each property document model.
//         /// </summary>
//         [Fact]
//         public void TypeMemberComments_AddedToDocModel()
//         {
//             var resourceDoc = XmlCommentsSetup.TypeService.GetResourceDoc(typeof(TestResource));
//             resourceDoc.Properties.Should().NotBeNullOrEmpty();
//
//             var firstPropDoc = resourceDoc.Properties.First(p => p.Name == "FirstValue");
//             firstPropDoc.Description.Should().Be("Example string property comment.");
//             firstPropDoc.Type.Should().BeOfType<string>().And.Subject.Should().Be("String");
//
//             var secondPropDoc = resourceDoc.Properties.First(p => p.Name == "SecondValue");
//             secondPropDoc.Description.Should().Be("Example integer string property.");
//             secondPropDoc.Type.Should().BeOfType<string>().And.Subject.Should().Be("Number");
//         }
//
//         /// <summary>
//         /// The required property is set on each resource property document model.  This is
//         /// based on the .NET Core Required attribute and the nullability of the property's
//         /// type.
//         /// </summary>
//         [Fact]
//         public void TypeMemberRequiredIndicator_SetOnDocModel()
//         {
//             var resourceDoc = XmlCommentsSetup.TypeService.GetResourceDoc(typeof(RequiredTestResource));
//             resourceDoc.Properties.First(p => p.Name == "FirstValue").IsRequired.Should().BeFalse();
//             resourceDoc.Properties.First(p => p.Name == "SecondValue").IsRequired.Should().BeTrue();
//             resourceDoc.Properties.First(p => p.Name == "ThirdValue").IsRequired.Should().BeTrue();
//             resourceDoc.Properties.First(p => p.Name == "ForthValue").IsRequired.Should().BeFalse();
//             resourceDoc.Properties.First(p => p.Name == "FifthValue").IsRequired.Should().BeTrue();
//             resourceDoc.Properties.First(p => p.Name == "SixthValue").IsRequired.Should().BeFalse();
//         }
//
//         /// <summary>
//         /// Based on the type of a resource's property, the IsArray property of the document
//         /// model is set.
//         /// </summary>
//         [Fact]
//         public void TypeMember_CanBeOf_ArrayType()
//         {
//             var resourceDoc = XmlCommentsSetup.TypeService.GetResourceDoc(typeof(ArrayTestResource));
//             var firstProp = resourceDoc.Properties.First(p => p.Name == "FirstValue");
//             var secondProp = resourceDoc.Properties.First(p => p.Name == "SecondValue");
//
//             firstProp.IsArray.Should().BeTrue();
//             firstProp.Type.Should().Be("Number");
//
//             secondProp.IsArray.Should().BeTrue();
//             secondProp.ResourceDoc.Should().NotBeNull();
//             secondProp.Type.Should().Be("Object");
//         }
//
//         /// <summary>
//         /// If a resource property is of a complex class type, the documentation for that
//         /// type is provided by setting the ResourceDoc property on the document model.
//         /// NOTE:  this is a recursive process.
//         /// </summary>
//         [Fact]
//         public void TypeMember_CanBeOf_ObjectType()
//         {
//             var resourceDoc = XmlCommentsSetup.TypeService.GetResourceDoc(typeof(TestResource));
//             resourceDoc.Properties.Should().HaveCount(3);
//
//             var objPropDoc = resourceDoc.Properties.First(p => p.Name == "ThirdValue");
//             objPropDoc.ResourceDoc.Should().NotBeNull();
//             objPropDoc.Type.Should().Be("Object");
//
//             var childObjDoc = objPropDoc.ResourceDoc;
//             childObjDoc.Properties.Should().NotBeNullOrEmpty();
//             childObjDoc.Properties.Should().HaveCount(1);
//
//             var childObjPropDoc = childObjDoc.Properties.First(p => p.Name == "FirstValue");
//             childObjPropDoc.Should().NotBeNull();
//             childObjPropDoc.Description.Should().Be("Example property of datetime.");
//         }
//     }
// }
