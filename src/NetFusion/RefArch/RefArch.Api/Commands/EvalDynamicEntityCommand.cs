using NetFusion.Messaging;
using RefArch.Api.Models;
using System.Collections.Generic;

namespace RefArch.Api.Commands
{
    public class EvalDynamicEntityCommand : Command<EvaluatedDomainModel>
    {
        public IDictionary<string, object> DynamicAttributes { get; set; }

        public EvalDynamicEntityCommand()
        {
            this.DynamicAttributes = new Dictionary<string, object>();
        }

        public bool IsActive { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public string ItemName { get; set; }
    }
}
