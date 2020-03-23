namespace NetFusion.Rest.Client
{
    public interface IRequestClientFactory
    {
        IRequestClient CreateClient(string name);
    }
}