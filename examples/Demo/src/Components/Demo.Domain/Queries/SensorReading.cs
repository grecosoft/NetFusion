using System.Collections.Generic;
using System.Runtime.Serialization;
using NetFusion.Base.Entity;

namespace Demo.Domain.Queries
{
    public class SensorReading : IAttributedEntity
    {
        public SensorReading()
        {
            Attributes = new EntityAttributes();
        }
        
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        
        [IgnoreDataMember]
        public IEntityAttributes Attributes { get; }
        
        public IDictionary<string, object> AttributeValues
        {
            get => Attributes.GetValues();
            set => Attributes.SetValues(value);
        }
    }
}