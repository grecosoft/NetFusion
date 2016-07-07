using RefArch.Api.Models;
using RefArch.Domain.Samples.WebApi;
using System.Collections.Generic;
using System.Linq;

namespace RefArch.Services.WebApi
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IDictionary<string, UserInfo> _users;

        public AuthenticationService()
        {
            // Obviously this should be stored in a database with password encryption.
            // But I don't want to miss House Hunters on HGTV. :)
            var users = new List<UserInfo>
            {
                new UserInfo("577db46fa27dc99098360a95", "beavis.smith@home.com", "Beavis", "Smith", "Accounting"),
                new UserInfo("577db470a27dc99098360a96", "lester.green@home.com", "Lester", "Green", "Software"),
                new UserInfo("577db472a27dc99098360a97", "dora.explorer@home.com", "Dora", "Explorer", "Purchasing"),
                new UserInfo("577db475a27dc99098360a98", "fred.flintstone@home.com", "Fred", "Flintstone", "Gravel-Pit")
            };

            _users = users.ToDictionary(u => u.Email);
        }

        public UserInfo IsValidLogin(string userName, string password)
        {
            return null;
        }
    }
}
