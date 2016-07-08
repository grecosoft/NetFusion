using NetFusion.Common;
using NetFusion.Common.Extensions;
using System;
using System.Linq;

namespace NetFusion.Messaging.Rules
{
    /// <summary>
    /// Associates a rule with a message handler that determines if the 
    /// handler should be called based on the message state.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ApplyDispatchRuleAttribute : Attribute
    {
        /// <summary>
        /// Determines how the rules are evaluated.
        /// </summary>
        public Type[] RuleTypes { get; private set; }

        /// <summary>
        /// The type of the role that should be called to determine if 
        /// the message handler method should be invoked.
        /// </summary>
        public RuleApplyTypes RuleApplyType { get; set; } = RuleApplyTypes.All;

        /// <summary>
        /// Constructor used to specify the dispatch rule types.
        /// </summary>
        /// <param name="dispatchRuleTypes">Determines how the rules are applied.</param>
        public ApplyDispatchRuleAttribute(params Type[] dispatchRuleTypes)
        {
            Check.NotNull(dispatchRuleTypes, nameof(dispatchRuleTypes));

            var invalidRules = dispatchRuleTypes
                .Where(drt => !drt.IsDerivedFrom<IMessageDispatchRule>())
                .Select(drt => drt.FullName);

            if (invalidRules.Any())
            {
                throw new InvalidOperationException(
                    $"The following are not dispatch rule types: {String.Join(", ", invalidRules)}" + 
                    $"-dispatch rules must derive from: {typeof(IMessageDispatchRule)}.");
            } 
      
            this.RuleTypes = dispatchRuleTypes;
        }
    }
}




