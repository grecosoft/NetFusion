using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Core.Descriptions
{
    /// <summary>
    /// Interface implemented by a class responsible for describing
    /// a parameter associated with an action method.
    /// </summary>
    public interface IParameterDescription : IDocDescription
    {
        /// <summary>
        /// Add documentation to an action method's parameter definition.
        /// </summary>
        /// <param name="parameterDoc">The created parameter document.</param>
        /// <param name="actionMeta">The action metadata containing the defined parameter.</param>
        /// <param name="parameterMeta">The metadata from which the document model was created.</param>
        void Describe(ApiParameterDoc parameterDoc, ApiActionMeta actionMeta, ApiParameterMeta parameterMeta);
    }
}