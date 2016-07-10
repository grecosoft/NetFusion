namespace RefArch.Api.Models
{
    public class UserInfo
    {
        public UserInfo(
            string userId,
            int maxPurchaseValue,
            int maxContractValue,
            int maxExposureValue)
        {
            this.UserId = userId;
            this.MaxPurchaseValue = maxPurchaseValue;
            this.MaxContractValue = maxContractValue;
            this.MaxExposureValue = maxExposureValue;
        }

        public string UserId { get; }
        public int MaxPurchaseValue { get; }
        public int MaxContractValue { get; }
        public int MaxExposureValue { get; }
    }
}
