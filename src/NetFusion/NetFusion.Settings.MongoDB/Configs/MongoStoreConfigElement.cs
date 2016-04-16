using System.Configuration;

namespace NetFusion.Settings.Mongo.Configs
{
    /// <summary>
    /// Configuration for specifying the source for application configurations 
    /// stored in MongoDb.
    /// </summary>
    public class MongoStoreConfigElement : ConfigurationElement
    {
        private readonly static ConfigurationProperty MongoUrlProp;
        private readonly static ConfigurationProperty DatabaseNameProp;
        private readonly static ConfigurationProperty CollectionNameProp;

        static MongoStoreConfigElement()
        {
            MongoUrlProp = new ConfigurationProperty("mongoUrl", typeof(string), null,
                ConfigurationPropertyOptions.IsRequired);

            DatabaseNameProp = new ConfigurationProperty("databaseName", typeof(string), null, 
                ConfigurationPropertyOptions.IsRequired);

            CollectionNameProp = new ConfigurationProperty("collectionName", typeof(string), null,
                ConfigurationPropertyOptions.IsRequired);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return new ConfigurationPropertyCollection
                {
                    MongoUrlProp,
                    DatabaseNameProp,
                    CollectionNameProp
                };
            }
        }

        /// <summary>
        /// The URL used to connect to the database.
        /// </summary>
        public string MongoUrl
        {
            get { return (string)this[MongoUrlProp]; }
            set { this[MongoUrlProp] = value; }
        }

        /// <summary>
        /// The database name in which the application settings are stored.
        /// </summary>
        public string DatabaseName
        {
            get { return (string)this[DatabaseNameProp]; }
            set { this[DatabaseNameProp] = value; }
        }

        /// <summary>
        /// The collection within the database were the configurations settings
        /// are stored.
        /// </summary>
        public string CollectionName
        {
            get { return (string)this[CollectionNameProp]; }
            set { this[CollectionNameProp] = value; }
        }
    }
}
