using NetFusion.Messaging.Types;

namespace Service.Domain.Commands
{
    public class CreateClaimSubmission : Command
    {
        public string InsuredId { get; set; }
        public string InsuredFistName { get; set; }
        public string InsuredLastName { get; set; }
        public decimal InsuredDeductible { get; set; }
        public decimal ClaimEstimate { get; set; }
        public string ClaimDescription { get; set; }
    }
}