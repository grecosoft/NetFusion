﻿using NetFusion.Messaging;
using NetFusion.Messaging.Rules;
using RefArch.Api.Messages;
using RefArch.Services.Messaging.Rules;

namespace RefArch.Services.Messaging
{
    public class ExampleRuleHandler : IMessageConsumer
    {
        [InProcessHandler, ApplyDispatchRule(typeof(IsLowImportance))]
        public void OnEvent([IncludeDerivedMessages]DomainEvent evt)
        {
            evt.Attributes["IsLowImportance"] = "Event is of low importance.";
        }

        [InProcessHandler, ApplyDispatchRule(typeof(IsHighImportance))]
        public void OnEvent(ExampleRuleDomainEvent evt)
        {
            evt.Attributes["IsHighImportance"] = "Event is of high importance.";
        }
    }
}