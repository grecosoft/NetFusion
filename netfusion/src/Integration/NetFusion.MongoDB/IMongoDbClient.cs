using MongoDB.Driver;
using System.Threading.Tasks;
using NetFusion.MongoDB.Settings;

namespace NetFusion.MongoDB
{
    /// <summary>
    /// Interface that is registered in the container used to access MongoDb.
    /// </summary>
    /// <typeparam name="TSettings"></typeparam>
    // ReSharper disable once UnusedTypeParameter
    public interface IMongoDbClient<out TSettings>
        where TSettings : MongoSettings
    {
        /// <summary>
        /// Reference to the underlying MongoDB.
        /// </summary>
        IMongoDatabase Database { get; }
        
        /// <summary>
        /// Returns a collection typed to a specific class.  
        /// </summary>
        /// <typeparam name="TDocument">The type of entity represented by the collection.</typeparam>
        /// <param name="name">The name of the collection.</param>
        /// <param name="settings">Optional collection settings.</param>
        /// <returns></returns>
        IMongoCollection<TDocument> GetCollection<TDocument>(string name, MongoCollectionSettings settings = null);

        /// <summary>
        /// Returns a collection typed to a specific class.  The name of the associated collection is
        /// determine based on the associated entity class mapping.  If a mapping for the class is not
        /// registered, the name of the class is used.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity represented by the collection.</typeparam>
        /// <param name="settings">Optional collection settings.</param>
        /// <returns>Collection typed to given class.</returns>
        IMongoCollection<TEntity> GetCollection<TEntity>(MongoCollectionSettings settings = null)
             where TEntity : class;

        /// <summary>
        /// Drops the collection associated with the specified entity type.
        /// </summary>
        /// <typeparam name="TEntity">The collection's entity type.</typeparam>
        /// <returns>Future result.</returns>
        Task DropCollectionAsync<TEntity>() where TEntity : class;
    }
}
