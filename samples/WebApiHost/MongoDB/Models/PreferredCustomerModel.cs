using System;

namespace WebApiHost.MongoDB.Models
{
    public class PreferredCustomerModel : CustomerModel
    {
        public string AccountNumber { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
