﻿using NetFusion.Messaging;
using RefArch.Api.Models;

namespace RefArch.Api.Messages
{
    public class ExampleRuleDomainEvent : DomainEvent
    {
        public int Value { get; }

        public ExampleRuleDomainEvent(MessageRuleInfo info) {
            this.Value = info.Value;

            if (this.Value == 50)
            {
                this.Attributes.SetValue("__low__", "");
            }
        }
    }
}
