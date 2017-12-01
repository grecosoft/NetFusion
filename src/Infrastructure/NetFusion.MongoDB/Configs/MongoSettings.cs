﻿using MongoDB.Driver;
using NetFusion.Settings;
using System.ComponentModel.DataAnnotations;

namespace NetFusion.MongoDB.Configs
{
    /// <summary>
    /// Setting uses by the MongoDB client when connecting to the database.
    /// Consuming applications derive from this base class to represent each
    /// MongoDb database being used.
    /// </summary>
    [ConfigurationSection("netfusion:plugins:mongoDB")]
    public abstract class MongoSettings : AppSettings
    {
        /// <summary>
        /// The URL used by the client when connecting to the database.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "MongoDB URL Required.")]
        public string MongoUrl { get; set; }

        /// <summary>
        /// User name used to authenticate with the database.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Password used to authenticate with the database.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The database used to authenticate.
        /// </summary>
        public string AuthDatabaseName { get; set; }

        /// <summary>
        /// The database to connect.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// This class can be used to specify the client settings rather than the MongoUrl.  
        /// </summary>
        public MongoClientSettings ClientSettings { get; set; }

        /// <summary>
        /// Optional database settings used when retrieving the database from the client.
        /// </summary>
        public MongoDatabaseSettings DatabaseSettings { get; set; }

        public bool IsPasswordSet
        {
            get {
                return (!string.IsNullOrWhiteSpace(AuthDatabaseName) || !string.IsNullOrWhiteSpace(DatabaseName))
                     && !string.IsNullOrWhiteSpace(UserName) 
                     && !string.IsNullOrWhiteSpace(Password);
            }
        }
    }
}