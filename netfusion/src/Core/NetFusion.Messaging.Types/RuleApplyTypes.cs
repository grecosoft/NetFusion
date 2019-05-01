namespace NetFusion.Messaging.Types
{
    /// <summary>
    /// Determines how dispatch rules should be evaluated when
    /// there are more than one dispatch rule.
    /// </summary> 
    public enum RuleApplyTypes
    {
        // All associated rules but return true.
        All = 0,

        // At least one rule must return true.
        Any = 1
    }
}
