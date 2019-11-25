namespace Demo.WebApi.Models
{
    public class StudentInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int[] Scores { get; set; }
        public int? PassingScore { get; set; }
    }
}
