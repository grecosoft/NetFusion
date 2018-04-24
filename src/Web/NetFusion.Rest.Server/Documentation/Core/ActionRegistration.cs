using System;
using System.Reflection;

namespace NetFusion.Rest.Server.Documentation.Core
{
    /// <summary>
    /// Class instance that is stored with reference to controller's action
    /// method and action-document to be lazy loaded.
    /// </summary>
    internal class ActionRegistration
    {
        /// <summary>
        /// The controller's action method to which the document corresponds.
        /// </summary>
        public MethodInfo ActionMethod { get; }

        /// <summary>
        /// The action method's documentation.
        /// </summary>
        public Lazy<DocAction> ActionDocument { get; }

        public ActionRegistration(
            MethodInfo actionMethod, 
            Lazy<DocAction> actionDoc)
        {
            ActionMethod = actionMethod;
            ActionDocument = actionDoc;
        }
    }
}
