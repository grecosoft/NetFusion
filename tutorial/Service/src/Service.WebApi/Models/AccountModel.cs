namespace Service.WebApi.Models
{
    /// <summary>
    /// Resource representing a summary of an account.
    /// </summary>
    public class AccountModel
    {
        /// <summary>
        /// The first name under which the account was established.
        /// </summary>
        public string FirstName { get; set; }
        
        /// <summary>
        /// The last name under which the account was established.
        /// </summary>
        public string LastName { get; set; }
        
        /// <summary>
        /// The account number assigned to the account.
        /// </summary>
        public string AccountNumber { get; set; }
    }
}