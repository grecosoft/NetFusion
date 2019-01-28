namespace IntegrationTests.Azure.Messaging
{
    using System;
    using NetFusion.Azure.Messaging.Subscriber;
    using Xunit;
    using FluentAssertions;

    public class SubscriptionSettingTests
    {
        [Fact]
        public void CanAddSubscriptionMapping()
        {
            var settings = new SubscriptionSettingsBase();
            var mapping = new SubscriptionMapping("T1", "S1", "MS1");
            
            
            settings.AddMapping("NS1", mapping);

            var mappedName = settings.GetMappedSubscription("NS1", mapping);
            mappedName.Should().NotBeNull();
            mappedName.Should().Be("MS1");
        }

        [Fact]
        public void CannotSpecifyDuplicationMapping()
        {
            var settings = new SubscriptionSettingsBase();
            var mapping = new SubscriptionMapping("T1", "S1", "MS1");

            Assert.Throws<InvalidOperationException>(
                () => settings.AddMapping("NS1", mapping, mapping));
        }
        
        [Fact]
        public void NonExistentMappingReturnsNull()
        {
            var settings = new SubscriptionSettingsBase();
            var mapping = new SubscriptionMapping("T1", "S1", "MS1");
            
            var mappedName = settings.GetMappedSubscription("NS1", mapping);
            mappedName.Should().BeNull();
        }
    }
}