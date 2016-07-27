using System.Collections.Generic;

namespace RefArch.Api.Models
{
    public class EvaluatedDomainModel
    {
        public EvaluatedDomainModel(DynamicDomainEntity domainEntity)
        {
            this.IsActive = domainEntity.IsActive;
            this.MinValue = domainEntity.MinValue;
            this.MaxValue = domainEntity.MaxValue;
            this.ItemName = domainEntity.ItemName;
            this.Attributes = domainEntity.Attributes; ;
        }

        public bool IsActive { get; }
        public int MinValue { get; }
        public int MaxValue { get; }
        public string ItemName { get; }
        public IDictionary<string, object> Attributes { get; }
    }
}
