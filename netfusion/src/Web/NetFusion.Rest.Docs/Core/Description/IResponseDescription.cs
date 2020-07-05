using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Core.Description
{
    /// <summary>
    /// Interface implemented by a class responsible for describing
    /// a controller's action response.
    /// </summary>
    public interface IResponseDescription : IDocDescription
    {
        /// <summary>
        /// Called to add descriptions to the specified action document.        
        /// </summary>
        /// <param name="responseDoc">The response document to describe.</param>
        /// <param name="responseMeta">The associated document metadata used
        /// to query related information.</param>
        void Describe(ApiResponseDoc responseDoc, ApiResponseMeta responseMeta);
    }
}