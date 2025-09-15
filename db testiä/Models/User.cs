using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace db_testiä.Models
{
    public class User
    {
        [BsonId]  // MongoDB:n ObjectId
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
    }
}
