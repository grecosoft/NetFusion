using RefArch.Api.Models;

namespace RefArch.Domain.Samples.WebApi
{
    public interface IAuthenticationService
    {
        UserLoginInfo ValidCredentials(string userName, string password);
    }
}
