namespace NetFusion.Domain.Patterns.Queries
{
    // TODO:
    public interface IPagedQuery<TResult> : IQuery<TResult>
    {
        PageCriteria PageCriteria { get; }
    }
}
