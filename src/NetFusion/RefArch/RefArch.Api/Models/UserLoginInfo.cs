namespace RefArch.Api.Models
{
    public class UserLoginInfo
    {
        public static UserLoginInfo InvalidUser => new UserLoginInfo();

        public string UserId { get; }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }

        public UserLoginInfo() { }

        public UserLoginInfo(
            string userId,
            string email,
            string firstName,
            string lastName)
        {
            this.UserId = userId;
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
        }
    }
}
