namespace NetFusion.Rest.Docs.Core.Description
{
    public static class DescriptionExtensions
    {
        public static T GetContextItem<T>(this IDocDescription description, string name)
        {
            if (description.Context.Properties.TryGetValue(name, out object value) && value is T item)
            {
                return item;
            }

            return default;
        }
    }
}