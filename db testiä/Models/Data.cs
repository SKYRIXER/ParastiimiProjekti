using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace db_testiä.Models
{
    public class Data
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("user_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("base_salary")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal BaseSalary { get; set; }

        [BsonElement("tax_percentage")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TaxPercentage { get; set; }

        [BsonElement("bonuses")]
        public List<Bonus> Bonuses { get; set; } = new();

        public class Bonus
        {
            [BsonElement("category")]
            public string Category { get; set; } = string.Empty;

            [BsonElement("amount")]
            [BsonRepresentation(BsonType.Decimal128)]
            public decimal Amount { get; set; }

            [BsonElement("is_active")]
            public bool IsActive { get; set; }
        }
    }
}
