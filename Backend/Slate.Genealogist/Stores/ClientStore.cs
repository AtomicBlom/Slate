using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using MongoDB.Driver;

namespace Slate.Genealogist.Stores
{
    public class MongoClientStore : IClientStore
    {
        private readonly IAuthDatabaseSettings _databaseConfiguration;

        public MongoClientStore(IAuthDatabaseSettings databaseConfiguration)
        {
            _databaseConfiguration = databaseConfiguration;
        }

        public async Task<Client?> FindClientByIdAsync(string clientId)
        {
            var dbClient = new MongoClient(_databaseConfiguration.ConnectionString);
            var database = dbClient.GetDatabase(_databaseConfiguration.DatabaseName);
            var clients = database.GetCollection<ClientEntry>(_databaseConfiguration.ClientsCollectionName);

            var results = await clients.FindAsync(c => c.Client.ClientId == clientId);
            var singleResult = await results.SingleOrDefaultAsync();
            return singleResult?.Client;
        }
    }
}
