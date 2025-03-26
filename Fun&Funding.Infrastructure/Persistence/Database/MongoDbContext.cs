using Fun_Funding.Domain.Entity.NoSqlEntities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Infrastructure.Persistence.Database
{
    public class MongoDBContext
    {
        private readonly IMongoDatabase _interactionDatabase;

        public MongoDBContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            _interactionDatabase = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));
        }

        // Generic method to get a collection for any entity type
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _interactionDatabase.GetCollection<T>(collectionName);
        }
    }

}
