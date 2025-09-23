using System.Collections.Generic;
using db_testiä.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace db_testiä.Services
{
    public class UserDataService
    {
        private readonly IMongoCollection<Data> _userData;

        public UserDataService(IOptions<MongoDBSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
            _userData = database.GetCollection<Data>("data");
        }

        public async Task<Data?> GetByUserIdAsync(string userId)
        {
            return await _userData.Find(data => data.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<Data> GetOrCreateForUserAsync(string userId)
        {
            var existing = await GetByUserIdAsync(userId);
            if (existing is not null)
            {
                return existing;
            }

            var document = new Data
            {
                UserId = userId,
                BaseSalary = 0m,
                TaxPercentage = 0m,
                Bonuses = new List<Data.Bonus>()
            };

            await _userData.InsertOneAsync(document);
            return document;
        }

        public async Task<Data> UpsertAsync(Data document)
        {
            if (string.IsNullOrWhiteSpace(document.UserId))
            {
                throw new ArgumentException("User identifier is required to save profile data.", nameof(document));
            }

            document.Bonuses ??= new List<Data.Bonus>();

            if (string.IsNullOrEmpty(document.Id))
            {
                await _userData.InsertOneAsync(document);
                return document;
            }

            await _userData.ReplaceOneAsync(data => data.Id == document.Id, document, new ReplaceOptions { IsUpsert = true });
            return document;
        }
    }
}
