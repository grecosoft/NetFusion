namespace RefArch.Api.Models
{
    public class UserInfo
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Unit { get; set; }

        public UserInfo(
            string userId,
            string email,
            string firstName,
            string lastName,
            string unit)
        {
            this.UserId = userId;
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Unit = unit;
        }
    }
}
