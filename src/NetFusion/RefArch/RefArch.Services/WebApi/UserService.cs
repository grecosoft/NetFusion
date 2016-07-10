using RefArch.Api.Models;
using RefArch.Domain.Samples.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RefArch.Services.WebApi
{
    public class UserService : IUserService
    {
        private IDictionary<string, UserInfo> _userInfo;

        public UserService()
        {
            var userInfo = new List<UserInfo>
            {
                new UserInfo("577db46fa27dc99098360a95", 100000, 50000, 8000),
                new UserInfo("577db470a27dc99098360a96", 200000, 70000, 8000),
                new UserInfo("577db472a27dc99098360a97", 600000, 90000, 10000),
                new UserInfo("577db475a27dc99098360a98", 800000, 10000, 10000),
            };

            _userInfo = userInfo.ToDictionary(u => u.UserId);

        }

        public UserInfo GetUserInfo(string userId)
        {
            var userInfo = _userInfo[userId];
            if (userInfo == null)
            {
                throw new InvalidOperationException(
                    "User information not found.");
            }

            return userInfo;
        }

        public IDictionary<string, UserInfo> ListUserInfo()
        {
            return _userInfo;
        }
    }
}
