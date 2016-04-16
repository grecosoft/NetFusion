using MongoDB.Driver;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
using NetFusion.Common.Extensions;
using NetFusion.MongoDB.Configs;
using NetFusion.MongoDB.Modules;
using System;
using System.Threading.Tasks;

namespace NetFusion.MongoDB.Core
{
    /// <summary>
    /// Service component that is registered with the dependency injection 
    /// container used to execute MongoDB commands.  Configures and delegates
    /// to an instance of the MongoClient class and adds additional services.
    /// </summary>
    /// <typeparam name="TSettings">Type defining the settings that
    /// should be used to connect to a specific database instance.
    /// </typeparam>
    public class MongoDbClient<TSettings> : IMongoDbClient<TSettings>,
        IComponentActivated
        where TSettings : MongoSettings
    {
        private readonly IMongoMappingModule _mappingModule;
        private IMongoClient _client;
        private IMongoDatabase _database;

        public TSettings DbSettings { get; }

        public MongoDbClient(TSettings dbSettings, IMongoMappingModule mappingModule)
        {
            Check.NotNull(dbSettings, nameof(dbSettings), "database settings not specified");
            Check.NotNull(mappingModule, nameof(mappingModule), "database mappings not specified");

            this.DbSettings = dbSettings;
            _mappingModule = mappingModule;
        }

        // Executed when the service component is activated.
        public virtual void OnActivated()
        {
            _client = CreateClient();

            _database = _client.GetDatabase(
                this.DbSettings.DatabaseName,
                this.DbSettings.DatabaseSettings);
        }

        public IMongoCollection<TDocument> GetCollection<TDocument>(string name,
            MongoCollectionSettings settings = null)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            return _database.GetCollection<TDocument>(name, settings);
        }

        public IMongoCollection<TEntity> GetCollection<TEntity>(MongoCollectionSettings settings = null)
            where TEntity : class
        {
            var entityType = typeof(TEntity);
            var mapping = _mappingModule.GetEntityMap(typeof(TEntity));

            return _database.GetCollection<TEntity>(mapping?.CollectionName ?? entityType.Name, settings);
        }

        public Task DropCollectionAsync<TEntity>()
            where TEntity : class
        {
            var entityType = typeof(TEntity);

            var mapping = _mappingModule.GetEntityMap(entityType);
            return _database.DropCollectionAsync(mapping?.CollectionName ?? entityType.Name);
        }

        private MongoClient CreateClient()
        {
            var clientSettings = CreateClientSettings(this.DbSettings);

            SetClientCredentials(clientSettings, this.DbSettings);
            return new MongoClient(clientSettings);
        }

        private MongoClientSettings CreateClientSettings(MongoSettings settings)
        {
            var clientSettings = this.DbSettings.ClientSettings;
            if (clientSettings == null)
            {
                if (this.DbSettings.MongoUrl.IsNullOrWhiteSpace())
                {
                    throw new InvalidOperationException(
                        "either the MongoDB URL or client settings must specified");
                }

                var url = MongoUrl.Create(this.DbSettings.MongoUrl);
                clientSettings = MongoClientSettings.FromUrl(url);

            }
            return clientSettings;
        }

        private void SetClientCredentials(MongoClientSettings clientSettings, MongoSettings settings)
        {
            MongoCredential credentials = null;
            if (this.DbSettings.IsPasswordSet)
            {
                credentials = MongoCredential.CreateMongoCRCredential(this.DbSettings.AuthDatabaseName,
                    this.DbSettings.UserName,
                    this.DbSettings.Password);

                clientSettings.Credentials = new[] { credentials };
            }
        }
    }
}
