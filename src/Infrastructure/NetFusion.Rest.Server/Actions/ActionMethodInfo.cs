namespace NetFusion.Rest.Server.Actions
{
    /// <summary>
    /// Contains information about a controller's action methods returned from a 
    /// search matching a set of conventions.
    /// </summary>
    public class ActionMethodInfo
    {
        /// <summary>
        /// The name of the controller.
        /// </summary>
        public string ControllerName { get; set; }

        /// <summary>
        /// The name of the controller's action method.
        /// </summary>
        public string ActionMethodName { get; set; }

        /// <summary>
        /// The HTTP methods supported by the action.
        /// </summary>
        public string[] Methods { get; set; }

        /// <summary>
        /// Contains the parameter information for the resource property and 
        /// action method argument determined to identity the resource.
        /// </summary>
        public ActionParamValue IdentityParamValue { get; set; }
    }
}
