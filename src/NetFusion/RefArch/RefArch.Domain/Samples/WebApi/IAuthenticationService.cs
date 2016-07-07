using RefArch.Api.Models;

namespace RefArch.Domain.Samples.WebApi
{
    public interface IAuthenticationService
    {
        UserInfo IsValidLogin(string userName, string password);
    }
}
