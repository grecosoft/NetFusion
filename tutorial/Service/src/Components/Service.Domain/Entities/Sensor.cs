using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NetFusion.Base.Entity;

namespace Service.Domain.Entities
{
    public class Sensor : IAttributedEntity
    {
        public Guid SensorId { get; set; }
        public string DisplayName { get; set; }
        public decimal CurrentValue { get; set; }
        public bool IsActiveAlert { get; set; }

        public Sensor()
        {
            Attributes = new EntityAttributes();
        }
        
        public Sensor(Guid sensorId, string displayName, decimal currentValue)
        {
            Attributes = new EntityAttributes();
            
            SensorId = sensorId;
            DisplayName = displayName;
            CurrentValue = currentValue;
        }

        /// <summary>
        /// Dynamic message attributes that can be associated with the domain-event.
        /// </summary>
        [IgnoreDataMember]
        public IEntityAttributes Attributes { get; }

        /// <summary>
        /// Dynamic message attribute values.
        /// </summary>
        public IDictionary<string, object> AttributeValues
        {
            get => Attributes.GetValues();
            set => Attributes.SetValues(value);
        }
    }
}