using NetFusion.Messaging;
using RefArch.Api.Commands;
using RefArch.Api.Models;
using RefArch.Domain.Samples.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RefArch.Services.WebApi
{
    public class AuthenticationService : IAuthenticationService,
        IMessageConsumer
    {
        private readonly IDictionary<string, UserAccount> _users;

        public AuthenticationService()
        {
            // Obviously this should be stored in a database with password encryption.
            // But I don't want to miss House Hunters on HGTV. :)
            var users = new List<UserAccount>
            {
                new UserAccount("577db46fa27dc99098360a95", "beavis.smith@home.com", "Beavis", "Smith", "P85$$3", "Accounting"),
                new UserAccount("577db470a27dc99098360a96", "lester.green@home.com", "Lester", "Green", "$%8434", "Software"),
                new UserAccount("577db472a27dc99098360a97", "dora.explorer@home.com", "Dora", "Explorer", "R6H422", "Purchasing"),
                new UserAccount("577db475a27dc99098360a98", "fred.flintstone@home.com", "Fred", "Flintstone", "A4G342", "Gravel-Pit")
            };

            _users = users.ToDictionary(u => u.Email);
        }

        public UserLoginInfo ValidCredentials(string userName, string password)
        {
            var user = _users[userName];
            if (user?.Password == password)
            {
                return user.ToUserLoginInfo();
            }

            return UserLoginInfo.InvalidUser;
        }

        [InProcessHandler]
        public UserLoginInfo OnValidCredentials(AuthorizeUserCommand command)
        {
            command.Result = ValidCredentials(command.UserName, command.Password);
            return command.Result;
        }
    }
}
