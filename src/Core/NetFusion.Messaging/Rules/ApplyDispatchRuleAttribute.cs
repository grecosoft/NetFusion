﻿using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Types.Rules;
using System;
using System.Linq;

namespace NetFusion.Messaging.Rules
{
    /// <summary>
    /// Associates a rule with a rule classes that determine if the 
    /// handler should be called based on the message's state.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ApplyDispatchRuleAttribute : Attribute
    {
        /// <summary>
        /// The types of the rules to be tested.
        /// </summary>
        public Type[] RuleTypes { get; private set; } 

        /// <summary>
        /// Specifies how the list of roles should be evaluated to 
        /// determine the final result.
        /// </summary>
        public RuleApplyTypes RuleApplyType { get; set; } = RuleApplyTypes.All;

        /// <summary>
        /// Constructor used to specify the dispatch rule types.
        /// </summary>
        /// <param name="dispatchRuleTypes">The types of the rules to be tested.</param>
        public ApplyDispatchRuleAttribute(params Type[] dispatchRuleTypes)
        {
            if (dispatchRuleTypes == null) throw new ArgumentNullException(nameof(dispatchRuleTypes));

            var invalidRules = dispatchRuleTypes
                .Where(drt => !drt.IsConcreteTypeDerivedFrom<IMessageDispatchRule>())
                .Select(drt => drt.FullName);

            if (invalidRules.Any())
            {
                throw new InvalidOperationException(
                    $"The following are not dispatch rule types: {String.Join(", ", invalidRules)}.  " + 
                    $"Dispatch rules must derive from: {typeof(IMessageDispatchRule)}.");
            } 
      
            RuleTypes = dispatchRuleTypes;
        }
    }
}




