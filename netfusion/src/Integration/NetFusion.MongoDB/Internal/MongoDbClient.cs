using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using NetFusion.MongoDB.Plugin;
using NetFusion.MongoDB.Settings;

namespace NetFusion.MongoDB.Internal
{
    /// <summary>
    /// Service component that is registered with the dependency injection container
    /// used to execute MongoDB commands.  Configures and delegates to an instance of
    /// the MongoClient class and adds additional services.
    /// </summary>
    /// <typeparam name="TSettings">Type defining the settings that should be used to connect
    /// to a specific database instance.
    /// </typeparam>
    public class MongoDbClient<TSettings> : IMongoDbClient<TSettings>
        where TSettings : MongoSettings
    {
        private readonly IMongoMappingModule _mappingModule;

        public TSettings DbSettings { get; }
        public IMongoDatabase Database { get; }

        public MongoDbClient(TSettings dbSettings, IMongoMappingModule mappingModule)
        {
            DbSettings = dbSettings ?? throw new ArgumentNullException(nameof(dbSettings));

            _mappingModule = mappingModule ?? throw new ArgumentNullException(nameof(mappingModule));
            
            IMongoClient client = CreateClient();

            Database = client.GetDatabase(
                dbSettings.DatabaseName,
                dbSettings.DatabaseSettings);
        }
        
        public IMongoCollection<TDocument> GetCollection<TDocument>(string collectionName,
            MongoCollectionSettings settings = null)
        {
            if (string.IsNullOrWhiteSpace(collectionName)) throw new ArgumentException(
                "Collection name must be specified.", nameof(collectionName));

            return Database.GetCollection<TDocument>(collectionName, settings);
        }

        public IMongoCollection<TEntity> GetCollection<TEntity>(MongoCollectionSettings settings = null)
            where TEntity : class
        {
            var mapping = GetEntityMapping(typeof(TEntity));
            var collectionName = GetEntityCollectionName(mapping);

            return Database.GetCollection<TEntity>(collectionName, settings);
        }

        public Task DropCollectionAsync<TEntity>()
            where TEntity : class
        {
            var mapping = GetEntityMapping(typeof(TEntity));
            var collectionName = GetEntityCollectionName(mapping);

            return Database.DropCollectionAsync(collectionName);
        }

        private IEntityClassMap GetEntityMapping(Type entityType)
        {
            var mapping = _mappingModule.GetEntityMap(entityType);
            if (mapping == null)
            {
                throw new InvalidOperationException(
                    $"A MongoDB mapping could not be found for type: {entityType}.");
            }
            return mapping;
        }

        private static string GetEntityCollectionName(IEntityClassMap mapping)
        {
            return mapping.CollectionName ?? mapping.EntityType.FullName;
        }

        private MongoClient CreateClient()
        {
            MongoClientSettings clientSettings = GetClientSettings();

            SetClientCredentials(clientSettings);
            return new MongoClient(clientSettings);
        }

        private MongoClientSettings GetClientSettings()
        {
            MongoClientSettings clientSettings = DbSettings.ClientSettings;
            if (clientSettings == null)
            {
                if (string.IsNullOrWhiteSpace(DbSettings.MongoUrl))
                {
                    throw new InvalidOperationException(
                        "Either the MongoDB URL or client settings must specified.");
                }

                var url = MongoUrl.Create(DbSettings.MongoUrl);
                clientSettings = MongoClientSettings.FromUrl(url);

            }
            return clientSettings;
        }

        private void SetClientCredentials(MongoClientSettings clientSettings)
        {
            if (DbSettings.IsPasswordSet)
            {
                var credentials = MongoCredential.CreateCredential(
                    DbSettings.AuthDatabaseName ?? DbSettings.DatabaseName,
                    DbSettings.UserName,
                    DbSettings.Password);

                clientSettings.Credential = credentials;
            }
        }
    }
}
