namespace NetFusion.Rest.Client
{
    /// <summary>
    /// Derived by an interface used to uniquely identify multiple
    /// IRequestClient instances used by a given consuming application.
    /// </summary>
    public interface IRequestClientService
    {
        /// <summary>
        /// Reference to the request client associated with the service.
        /// </summary>
        IRequestClient Client { get; }
    }
}