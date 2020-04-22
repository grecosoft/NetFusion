//namespace IntegrationTests.Serialization
//{
//    using System.Text;
//    using FluentAssertions;
//    using NetFusion.Base;
//    using NetFusion.Messaging.Types;
//    using NetFusion.Serialization;
//    using Xunit;
//
//    public class JsonMessageSerializerTests
//    {
//        [Fact]
//        public void PrivatePropertiesWithSettersSerialized()
//        {
//            var de = new MockDomainEvent("Tom", "Green");
//            var mgr = new SerializationManager();
//
//            var json = mgr.Serialize(de, ContentTypes.Json);
//            var de2 = mgr.Deserialize<MockDomainEvent>(ContentTypes.Json, json);
//
//            de2.FirstName.Should().Be(de.FirstName);
//            de2.LastName.Should().Be(de.LastName);
//        }
//
//        // An Attributed Entity can have an associated dynamic set of attributes
//        // containing non-typed property values.  When Serialized as json, the
//        // Attributes property is not serialized directly.  It is serialized and
//        // deserialized indirectly via the AttributeValues getter/setter.
//        [Fact]
//        public void AttributedEntity_DynamicProperty_NotSerialized()
//        {
//            var de = new MockDomainEvent("Tom", "Green");
//            var mgr = new SerializationManager();
//
//            de.Attributes.Values.State = "PA";
//            
//            var json = mgr.Serialize(de, ContentTypes.Json);
//            var jsonString = Encoding.UTF8.GetString(json);
//
//            jsonString.Should().NotContain("Attributes");
//
//            var de2 = mgr.Deserialize<MockDomainEvent>(ContentTypes.Json, json);
//            string state = de2.Attributes.Values.State;
//
//            state.Should().Be("PA");
//        }
//        
//        public class MockDomainEvent : DomainEvent
//        {
//            public string FirstName { get; set; }
//            public string LastName { get; private set; }
//
//            public MockDomainEvent()
//            {
//            }
//
//            public MockDomainEvent(string firstName, string lastName)
//            {
//                FirstName = firstName;
//                LastName = lastName;
//            }
//        }
//    }
//}