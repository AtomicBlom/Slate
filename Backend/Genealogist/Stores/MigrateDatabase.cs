using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace Genealogist.Stores
{
    public class MigrateDatabase : IMigrateDatabase
    {
        private readonly IAuthDatabaseSettings _databaseConfiguration;

        public MigrateDatabase(IAuthDatabaseSettings databaseConfiguration)
        {
            _databaseConfiguration = databaseConfiguration;
        }

        public async Task Migrate()
        {
            var launcherId = "Launcher";
            var launcherClient = new Client
            {
                ClientId = launcherId,
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                AllowOfflineAccess = true,
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                AllowedScopes = { "account", "offline_access" }
            };
            var gameWardenClient = new Client
            {
                ClientId = "GameWarden",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
            };
            
            var dbClient = new MongoClient(_databaseConfiguration.ConnectionString);
            var database = dbClient.GetDatabase(_databaseConfiguration.DatabaseName);
            var clients = database.GetCollection<ClientEntry>(_databaseConfiguration.ClientsCollectionName);
            
            await UpsertClient(clients, launcherClient);
            await UpsertClient(clients, gameWardenClient);
        }

        private static async Task UpsertClient(IMongoCollection<ClientEntry> clientCollection, Client client)
        {
            var launcherClientCursor = await clientCollection.FindAsync(f => f.Client.ClientId == client.ClientId);
            var launcherClientEntry = await launcherClientCursor.SingleOrDefaultAsync();
            if (launcherClientEntry is null)
            {
                launcherClientEntry = new ClientEntry
                {
                    Id = ObjectId.GenerateNewId(),
                    Client = client
                };
            }
            else
            {
                launcherClientEntry.Client = client;
            }

            await clientCollection.ReplaceOneAsync(f => f.Client.ClientId == client.ClientId, launcherClientEntry,
                new ReplaceOptions { IsUpsert = true });
        }
    }

    public class ClientEntry
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        public ObjectId Id { get; set; }
        public Client Client { get; set; }
    }
    
    public interface IMigrateDatabase
    {
        public Task Migrate();
    }

    public class AuthDatabaseSettings : IAuthDatabaseSettings
    {
        public string ClientsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IAuthDatabaseSettings
    {
        string ClientsCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }

}
