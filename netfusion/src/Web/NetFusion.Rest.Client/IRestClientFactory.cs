namespace NetFusion.Rest.Client
{
    // 
    public interface IRestClientFactory
    {
        IRestClient CreateClient(string name);
    }
}