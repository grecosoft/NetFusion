using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentAssertions;
using NetFusion.Rest.Docs.Models;
using WebTests.Rest.DocGeneration.Mocks;
using Xunit;

namespace WebTests.Rest.DocGeneration
{
    public class XmlTypeCommentTests
    {
        [Fact]
        public void TypeComments_AddedToDocModel()
        {
            var resourceDoc = XmlTypeCommentMock.Arrange.GetResourceDoc(typeof(TestResource));
            resourceDoc.Should().NotBeNull();
            resourceDoc.Description.Should().Be("Class comment for test-resource.");
        }

        [Fact]
        public void TypeMemberComments_AddedToDocModel()
        {
            var resourceDoc = XmlTypeCommentMock.Arrange.GetResourceDoc(typeof(TestResource));
            resourceDoc.Properties.Should().NotBeNullOrEmpty();
            resourceDoc.Properties.Should().HaveCount(3);

            var firstPropDoc = resourceDoc.Properties.FirstOrDefault(p => p.Name == "FirstValue");
            firstPropDoc.Should().NotBeNull();
            firstPropDoc.Description.Should().Be("Example string property comment.");
            firstPropDoc.Type.Should().BeOfType<string>().And.Subject.Should().Be("String");

            var secondPropDoc = resourceDoc.Properties.FirstOrDefault(p => p.Name == "SecondValue");
            secondPropDoc.Should().NotBeNull();
            secondPropDoc.Description.Should().Be("Example integer string property.");
            secondPropDoc.Type.Should().BeOfType<string>().And.Subject.Should().Be("Number");
        }

        [Fact]
        public void TypeMemberRequiredIndicator_SetOnDocModel()
        {
            var resourceDoc = XmlTypeCommentMock.Arrange.GetResourceDoc(typeof(RequiredTestResource));
            resourceDoc.Properties.FirstOrDefault(p => p.Name == "FirstValue").IsRequired.Should().BeFalse();
            resourceDoc.Properties.FirstOrDefault(p => p.Name == "SecondValue").IsRequired.Should().BeTrue();
            resourceDoc.Properties.FirstOrDefault(p => p.Name == "ThirdValue").IsRequired.Should().BeTrue();
            resourceDoc.Properties.FirstOrDefault(p => p.Name == "ForthValue").IsRequired.Should().BeFalse();
            resourceDoc.Properties.FirstOrDefault(p => p.Name == "FifthValue").IsRequired.Should().BeTrue();
            resourceDoc.Properties.FirstOrDefault(p => p.Name == "SixthValue").IsRequired.Should().BeFalse();
        }

        [Fact]
        public void TypeMember_CanBeOf_ArrayType()
        {
            var resourceDoc = XmlTypeCommentMock.Arrange.GetResourceDoc(typeof(ArrayTestResource));
            var firstProp = resourceDoc.Properties.FirstOrDefault(p => p.Name == "FirstValue");
            var secondProp = resourceDoc.Properties.FirstOrDefault(p => p.Name == "SecondValue");

            firstProp.IsArray.Should().BeTrue();
            firstProp.Type.Should().Be("Number");

            secondProp.IsArray.Should().BeTrue();
            secondProp.ResourceDoc.Should().NotBeNull();
            secondProp.Type.Should().Be("Object");
        }

        [Fact]
        public void TypeMember_CanBeOf_ObjectType()
        {
            var resourceDoc = XmlTypeCommentMock.Arrange.GetResourceDoc(typeof(TestResource));
            resourceDoc.Properties.Should().HaveCount(3);

            var objPropDoc = resourceDoc.Properties.FirstOrDefault(p => p.Name == "ThirdValue");
            objPropDoc.Should().NotBeNull();
            objPropDoc.ResourceDoc.Should().NotBeNull();
            objPropDoc.Type.Should().Be("Object");

            var childObjDoc = objPropDoc.ResourceDoc;
            childObjDoc.Properties.Should().NotBeNullOrEmpty();
            childObjDoc.Properties.Should().HaveCount(1);

            var childObjPropDoc = childObjDoc.Properties.FirstOrDefault(p => p.Name == "FirstValue");
            childObjPropDoc.Should().NotBeNull();
            childObjPropDoc.Description.Should().Be("Example property of datetime.");
        }
    }

    /// <summary>
    /// Class comment for test-resource.
    /// </summary>
    public class TestResource
    {
        /// <summary>
        /// Example string property comment.
        /// </summary>
        public string FirstValue { get; set; }

        /// <summary>
        /// Example integer string property.
        /// </summary>
        public int SecondValue { get; set; }

        /// <summary>
        /// Example property of an object type.
        /// </summary>
        public TestChildResource ThirdValue { get; set; }
    }

    /// <summary>
    /// Example comment for a child object type.
    /// </summary>
    public class TestChildResource
    {
        /// <summary>
        /// Example property of datetime.
        /// </summary>
        public DateTime FirstValue { get; set; }
    }

    public class RequiredTestResource
    {
        public string FirstValue { get; set; }

        [Required]
        public string SecondValue { get; set; }

        public int ThirdValue { get; set; }

        public DateTime? ForthValue { get; set; }

        [Required]
        public int? FifthValue { get; set; }

        public TestChildResource SixthValue { get; set; }
    }

    public class ArrayTestResource
    {
        public int[] FirstValue { get; set; }

        public ArrayItemType[] SecondValue { get; set; }
    }

    public class ArrayItemType
    {

    }
}
