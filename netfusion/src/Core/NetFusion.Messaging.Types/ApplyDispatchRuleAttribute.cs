using System;
using System.Linq;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Messaging.Types
{
    /// <summary>
    /// Used to specify rule classes that determine if the handler 
    /// should be called based on the message's state.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
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
                .Select(drt => drt.FullName)
                .ToArray();

            if (invalidRules.Any())
            {
                throw new InvalidOperationException(
                    $"The following are not dispatch rule types: {string.Join(", ", invalidRules)}.  " + 
                    $"Dispatch rules must derive from: {typeof(IMessageDispatchRule)}.");
            } 
      
            RuleTypes = dispatchRuleTypes;
        }
    }
}




