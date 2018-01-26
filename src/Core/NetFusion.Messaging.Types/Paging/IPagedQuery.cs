namespace NetFusion.Messaging.Types.Paging
{
    // TODO:
    public interface IPagedQuery<TResult> : IQuery<TResult>
    {
        PageCriteria PageCriteria { get; }
    }
}
