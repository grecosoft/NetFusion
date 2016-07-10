using RefArch.Api.Models;
using System.Collections.Generic;

namespace RefArch.Domain.Samples.WebApi
{
    public interface IUserService
    {
        UserInfo GetUserInfo(string userId);
        IDictionary<string, UserInfo> ListUserInfo();
    }
}
