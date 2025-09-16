using db_testiä.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace db_testiä.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IOptions<MongoDBSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
            _users = database.GetCollection<User>("users");
        }

        public async Task<List<User>> GetAsync()
        {
            return await _users.Find(user => true).ToListAsync();
        }

        public async Task<User?> GetAsync(string id)
        {
            return await _users.Find(user => user.Id == id).FirstOrDefaultAsync();
        }


        public async Task<User?> GetByNameAsync(string name)

        {
            return await _users.Find(user => user.Name == name).FirstOrDefaultAsync();
        }

        public async Task<User> CreateAsync(User user)
        {
            await _users.InsertOneAsync(user);
            return user;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _users.DeleteOneAsync(user => user.Id == id);
            return result.DeletedCount > 0;
        }
    }

    public class MongoDBSettings
    {
        public string? ConnectionString { get; set; }
        public string? DatabaseName { get; set; }
    }
}
