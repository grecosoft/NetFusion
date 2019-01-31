namespace IntegrationTests.Azure.Messaging
{
    using System;
    using Xunit;
    using FluentAssertions;
    using NetFusion.AMQP.Subscriber;

    public class SubscriptionSettingTests
    {
        [Fact]
        public void CanAddSubscriptionMapping()
        {
            var settings = new SubscriptionSettingsBase();
            var mapping = new SubscriptionMapping("NS1", "T1", "S1", "MS1");
            
            
            settings.AddMapping(mapping);

            var mappedName = settings.GetMappedSubscription(mapping);
            mappedName.Should().NotBeNull();
            mappedName.Should().Be("MS1");
        }

        [Fact]
        public void CannotSpecifyDuplicationMapping()
        {
            var settings = new SubscriptionSettingsBase();
            var mapping = new SubscriptionMapping("NS1", "T1", "S1", "MS1");

            Assert.Throws<InvalidOperationException>(
                () => settings.AddMapping(mapping, mapping));
        }
        
        [Fact]
        public void NonExistentMappingReturnsNull()
        {
            var settings = new SubscriptionSettingsBase();
            var mapping = new SubscriptionMapping("NS1", "T1", "S1", "MS1");
            
            var mappedName = settings.GetMappedSubscription(mapping);
            mappedName.Should().BeNull();
        }
    }
}