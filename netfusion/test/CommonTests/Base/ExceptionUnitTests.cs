using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NetFusion.Base.Exceptions;
using Xunit;
// ReSharper disable All

namespace CommonTests.Base
{
    /// <summary>
    /// These unit-test validate the setting of exception details.  The base NetFusionException has
    /// a Details property containing a dictionary of releated exception details.  These details can
    /// be directly specified by the caller and/or populated based on the types of the InnerException
    /// or list of related exceptions.  If these releated exceptions are derived from NetFusionException,
    /// their details are added to the parent exception details.  When the parent exception is logged
    /// using Serilog, a structured property name Details is added and set to the Details property
    /// for the logged exception.  This provided a very detailed and organzied error log.
    ///
    /// NOTE:  Some of these exception unit-test might seem a little mysterious or obscure but they
    /// are validating the structure of the detailed property.  Unlike these tests, a developer will
    /// never program against the Details property which will always be viewed as a simple JSON log.
    /// </summary>
    public class ExceptionUnitTests
    {
        [Fact]
        public void Message_AddedToDetails()
        {
            var ex = new MockException("TestMessage"); 
            ex.Details["Message"].ToString().Should().Be("TestMessage");
        }
        
        [Fact]
        public void MessageAndInnerException_AddedToDetails()
        {
            var ex = new MockException("TestMessage", new InvalidOperationException("InnerTestMessage"));
            
            ex.Details["Message"].ToString().Should().Be("TestMessage");
            ex.Details["InnerException"].ToString().Should().Contain("InnerTestMessage");
        }

        [Fact]
        public void NetFusionInnerException_DetailsAdded_ToParentDetails()
        {
            // Arrange:
            var innerEx = new MockException("InnerTestMessage", "AdditionalValues", new {a = "value", b = 400});
            var ex = new MockException("TestMessage", innerEx);

            // Assert:
            ex.Details.Should().ContainKey("InnerDetails");

            var innerDetails = ex.Details["InnerDetails"] as Dictionary<string, object>;
            innerDetails.Should().NotBeNull();

            dynamic details = innerDetails["AdditionalValues"];
            Assert.Equal("value", details.a);
            Assert.Equal(400, details.b);
        }

        [Fact]
        public void RelatedNetFusionExceptions_DetailsAdded_ToParent()
        {
            // Arrange:
            var innerEx = new InvalidOperationException("InnerTestMessage");
            var ex = new MockException("TestMessage", innerEx, new Exception[]
            {
                new InvalidOperationException("ChildMessageEx1"),
                new MockException("ChildMessageEx2", "AdditionalValues", new {a = "value", b = 400})
            });
            
            // Assert:
            ex.ChildExceptions.Should().HaveCount(2);

            // Since the ChildExceptions is a list of exceptions, the details for each NetFusion derived
            // exception is set to "InnerDetails" - which is a list.
            var listOfDetails = ex.Details["InnerDetails"] as IEnumerable<object>;
            listOfDetails.Should().NotBeNull();
            listOfDetails.Should().HaveCount(1);

            // Since only one child exception derived from NetFusionException, the list of details
            // will only have one item.
            var innerDetails = listOfDetails.ElementAt(0) as Dictionary<string, object>;
            innerDetails.Should().NotBeNull();

            dynamic details = innerDetails["AdditionalValues"];
            Assert.Equal("value", details.a);
            Assert.Equal(400, details.b);
        }
        
        [Fact]
        public void AggregateInnerException_InnerExceptions_AddedToParentDetails()
        {
            
        }

        private class MockException : NetFusionException
        {
            public MockException(string message)
                : base(message)
            {
                
            }
            
            public MockException(string message, Exception innerException)
                : base(message, innerException)
            {
                
            }

            public MockException(string message, string detailKey, object details)
                : base(message, detailKey, details)
            {
                
            }

            public MockException(string message, Exception innerException, IEnumerable<Exception> relatedExceptions)
                : base(message, innerException)
            {
                SetChildExceptions(relatedExceptions);                
            }
        }
    }
}