using NetFusion.Base.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Domain.Patterns.UnitOfWork
{
    /// <summary>
    /// The results of committing the unit-of-work.
    /// </summary>
    public class CommitResult : IDisposable
    {
        private bool _disposed = false;
        private IAggregateUnitOfWork _unitOfWork;

        /// <summary>
        /// Indicates if the commit was successful.
        /// </summary>
        public bool IsSucessful { get; private set; }

        /// <summary>
        /// Indicates that the unit-of-work was not committed because an enlisted 
        /// aggregate had error validations.
        /// </summary>
        public bool HasErrors { get; private set; }

        /// <summary>
        /// Indicates the committed unit-of-work has associated validations.
        /// </summary>
        public bool HasValidations => ValidationResults.Any();

        /// <summary>
        /// The validations for any of the enlisted aggregates.
        /// </summary>
        public IEnumerable<ValidationResultSet> ValidationResults { get; private set; }

        internal static CommitResult Sucessful(IAggregateUnitOfWork unitOfwork, IEnumerable<ValidationResultSet> validationResults)
        {
            if (unitOfwork == null) throw new ArgumentNullException(nameof(unitOfwork));
            if (validationResults == null) throw new ArgumentNullException(nameof(validationResults));

            return new CommitResult
            {
                _unitOfWork = unitOfwork,

                IsSucessful = true,
                HasErrors = false,
                ValidationResults = new List<ValidationResultSet>(validationResults)
            };
        }

        internal static CommitResult Invalid(IAggregateUnitOfWork unitOfwork, IEnumerable<ValidationResultSet> validationResults)
        {
            if (unitOfwork == null) throw new ArgumentNullException(nameof(unitOfwork));
            if (validationResults == null) throw new ArgumentNullException(nameof(validationResults));

            return new CommitResult {

                _unitOfWork = unitOfwork,

                IsSucessful = false,
                HasErrors = true,
                ValidationResults = new List<ValidationResultSet>(validationResults)
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _disposed) return;

            _unitOfWork?.Clear();            
            _disposed = true;
        }
    }
}
