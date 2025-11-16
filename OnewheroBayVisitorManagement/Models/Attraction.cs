using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OnewheroBayVisitorManagement.Models
{
    public class Attraction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("openingHours")]
        public string OpeningHours { get; set; }

        [BsonElement("imageUrl")]
        public string ImageUrl { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; }

        public Attraction()
        {
            IsActive = true;
        }
    }
}