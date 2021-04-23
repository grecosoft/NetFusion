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
        
        /// <summary>
        /// As with the prior unit-test, if an AggregateException is passed, all NetFusionException
        /// derived exceptions will have their Details added to the parent exception after calling
        /// the Flatten method.  This unit-test creates an aggregate exception and asserts that all
        /// child exception details are added.
        /// </summary>
        [Fact]
        public void AggregateInnerException_InnerExceptions_AddedToParentDetails()
        {
            // Arrange:
            var aggEx = new AggregateException("AggregateException1",
          
                new MockException("InnerException1"),
                new AggregateException("AggregateException2",
                    
                    new InvalidOperationException("InnerException2"),
                    new AggregateException("AggregateException3", 
                        new Exception[] { new MockException("InnerException3")})
                )
            );

            var ex = new MockException("ParentExceptionMessage", aggEx);

            // Assert:
            // All three child aggregate exceptions specifed above will be 
            // added to the collection.
            ex.ChildExceptions.Should().HaveCount(3);
            
            // Since 2 out of the 3 exceptions are NetFusionException derived, 
            // they will only appear in the details.
            var listOfDetails = ex.Details["InnerDetails"] as IEnumerable<object>;
            listOfDetails.Should().NotBeNull();
            listOfDetails.Should().HaveCount(2);

            var details1 = (Dictionary<string, object>) listOfDetails.ElementAt(0);
            var details2 = (Dictionary<string, object>) listOfDetails.ElementAt(1);

            details1["Message"].Should().Be("InnerException1");
            details2["Message"].Should().Be("InnerException3");
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

            public MockException(string message, AggregateException aggregateException)
                : base(message, aggregateException)
            {
                
            }
        }
    }
}