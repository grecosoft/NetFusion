using System.Collections.Generic;

namespace NetFusion.Domain.ReferenceTypes.Core
{
    /// <summary>
    /// Repository contract for reference types.
    /// </summary>
    public interface IReferenceTypeRepository
    {
        /// <summary>
        /// Adds a reference type to the repository.  This method is 
        /// typically used by a initialization script to seed the 
        /// database.
        /// </summary>
        /// <param name="referenceType">The reference type to add.</param>
        void AddReferenceType(ReferenceType referenceType);

        /// <summary>
        /// Returns all configured reference types.  This method is typically
        /// called during the application's bootstrap process.  RefereceTypeModule
        /// invokes this method and sets a static reference on ReferenceType.
        /// </summary>
        /// <returns>List of all configured reference types.</returns>
        IEnumerable<ReferenceType> GetReferenceTypes();
    }
}
