using RefArch.Api.Models;

namespace RefArch.Domain.Samples.WebApi
{
    public class UserAccount
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Password { get; set; }
        public string Unit { get; set; }

        private UserAccount() { }

        public UserAccount(
            string userId,
            string email,
            string firstName,
            string lastName,
            string password,
            string unit)
        {
            this.UserId = userId;
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Password = password;
            this.Unit = unit;
        }

        public UserLoginInfo ToUserLoginInfo()
        {
            return new UserLoginInfo(UserId, Email, FirstName, LastName);
        }
    }
}