using NetFusion.Messaging;
using RefArch.Api.Models;

namespace RefArch.Api.Commands
{
    public class AuthorizeUserCommand : Command<UserLoginInfo>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
