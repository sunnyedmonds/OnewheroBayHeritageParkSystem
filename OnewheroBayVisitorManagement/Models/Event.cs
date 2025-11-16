using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OnewheroBayVisitorManagement.Models
{
    public class Event
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("eventName")]
        public string EventName { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("eventDate")]
        public DateTime EventDate { get; set; }

        [BsonElement("eventTime")]
        public string EventTime { get; set; }

        [BsonElement("location")]
        public string Location { get; set; }

        [BsonElement("capacity")]
        public int Capacity { get; set; }

        [BsonElement("availableSeats")]
        public int AvailableSeats { get; set; }

        [BsonElement("ticketPrice")]
        public decimal TicketPrice { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("imageUrl")]
        public string ImageUrl { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; }

        public Event()
        {
            IsActive = true;
            AvailableSeats = Capacity;
        }
    }
}