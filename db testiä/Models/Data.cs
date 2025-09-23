using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace db_testiä.Models
{
    public class Data
    {
        [BsonId]  // MongoDB:n ObjectId.
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string user_id { get; set; } = string.Empty;
        public float base_salary { get; set; }
        public int tax_percentage { get; set; }

        public Bonus[] bonuses;
        public struct Bonus()
        {
          public string category { get; set; }
          public float amount { get; set; }
          public bool is_active { get; set; }

        }
    }
}
