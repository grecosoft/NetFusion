using NetFusion.Messaging.Types;
using NetFusion.RabbitMQ.Serialization;
using Newtonsoft.Json;
using Xunit;

namespace IntegrationTests.RabbitMQ
{
    public class SerializationTests
    {
        [Fact]
        public void CanSerializeProperties_WithPrivateSetters()
        {
            var msgSerializer = new JsonMessageSerializer();
            
            var m = new Model
            {
                Value1 = 1000,
                Value2 = 2000
            };
            
            var msg = new TestMessage(m);
            msg.Attributes.SetValue("v1", 8000);
            
            byte[] v = msgSerializer.Serialize(msg);

            var msg2 = msgSerializer.Deserialize<TestMessage>(v, typeof(TestMessage));
        }
        
        public class TestMessage : DomainEvent
        {
            public TestMessage()
            {
                
            }
            
            public TestMessage(Model m)
            {
                Value1 = m.Value1;
                Value2 = m.Value2;
            }
            
            public int Value1 { get; private set; }
            public int Value2 { get; private set;  }
        }

        public class Model
        {
            public int Value1 { get; set; }
            public int Value2 { get; set; }
        }
    }
}