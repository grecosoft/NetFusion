namespace WebApiHost.Models
{
    public class StudentSummary : ContactSummary
    {
        public int MaxScore { get; set; }
        public int MinScore { get; set; }
    }
}
