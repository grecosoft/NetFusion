using System;
using System.ComponentModel.DataAnnotations;

namespace WebTests.Rest.DocGeneration.Server
{
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

        /// <summary>
        /// Comment associated with a method.
        /// </summary>
        public void TestMethod()
        {
            
        }
        
        /// <summary>
        /// Comment associated with a method.
        /// </summary>
        /// <param name="itemId">Comment associated with a parameter.</param>
        public void TestMethodWithParam(int itemId)
        {
            
        }
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