using System;

namespace NetFusion.Base.Entity
{
    /// <summary>
    /// Class instance associated with the current request used to track concurrency information.
    /// The state of this class is initialized from the request header and used to determine the
    /// response headers and status codes.
    /// </summary>
    public class EntityContext
    {
        private string _clientToken;
        
        /// <summary>
        /// The concurrency token submitted by the client when updating an entity.
        /// This property is populated with the value of the If-Match Http Header.
        /// </summary>
        public string ClientToken
        {
            get => _clientToken ?? throw new InvalidOperationException("Client Token has not been set.");
            set => _clientToken = value;
        }
        
        /// <summary>
        /// The current token associated with an entity.  When successfully updating an entity,
        /// this property can be set to the entity's newly generated token value.  If an update
        /// fails due to a concurrency check, this property will be set to the entity's current
        /// token value. 
        /// </summary>
        public string CurrentToken { get; private set; }
        
        /// <summary>
        /// Reference to the entity being updated and associated with the client-token.
        /// </summary>
        public object Entity { get; private set; }
        
        /// <summary>
        /// Indicates the state of the concurrency context meets the preconditions
        /// required to detect changed made to an entity since read.
        /// </summary>
        public bool PreConditionSatisfied { get; private set; } = true;

        /// <summary>
        /// Indicates that the current request does not have an associated entity requiring
        /// concurrency checking when updated or was successfully updated.
        /// </summary>
        public bool ConcurrencyCheckSatisfied { get; private set; } = true;
        
        /// <summary>
        /// The current state of the entity when an update is attempted and a concurrency check fails.
        /// Can be used by the caller to determine the appropriate corrective action. 
        /// </summary>
        public object CurrentEntityState { get; private set; }

        /// <summary>
        /// Sets the concurrency token value submitted by the caller received
        /// when initially reading the entity.
        /// </summary>
        /// <param name="token">The value of the token.</param>
        public void SetClientToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Client-token value not specified.", nameof(token));
            
            if (ClientToken != null)
            {
                throw new InvalidOperationException("The client-token can only be set once per request.");
            }

            ClientToken = token;
        }

        /// <summary>
        /// Reference to the updated entity corresponding to the current client-token.
        /// </summary>
        /// <param name="entity">Reference containing the updated entity's state.</param>
        /// <returns>Reference to the updated entity state.</returns>
        public bool SetEntity(object entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            
            // If the Api indicates that the entity requires concurrency checking when
            // updating but the client has not submitted an associated client-token,
            // the preconditions have not been satisfied and the Api should not proceed
            // in applying updates.
            if (ClientToken == null)
            {
                return PreConditionSatisfied = false;
            }
            
            if (Entity != null)
            {
                throw new InvalidOperationException("The entity can only be set once per request.");
            }

            Entity = entity;
            return true;
        }
        
        /// <summary>
        /// Records information associated with a failed entity update returned to the client
        /// used to determine how to best resolve the update failure.
        /// </summary>
        /// <param name="currentToken">The current token associated when the entity.</param>
        /// <param name="currentEntityState">The optional current state of the entity.</param>
        public void RecordFailedUpdate(string currentToken, object currentEntityState = null)
        {
            if (string.IsNullOrWhiteSpace(currentToken))
                throw new ArgumentException("Current concurrency token not specified.", nameof(currentToken));
            
            ConcurrencyCheckSatisfied = false;
            CurrentToken = currentToken;
            CurrentEntityState = currentEntityState;
        }

        /// <summary>
        /// Indicates that the entity was successfully updated.
        /// </summary>
        /// <param name="updatedToken">The newly generated token associated with the updated entity.</param>
        public void RecordSuccessfulUpdate(string updatedToken)
        {
            ConcurrencyCheckSatisfied = true;
            CurrentToken = updatedToken;
        }
    }
}