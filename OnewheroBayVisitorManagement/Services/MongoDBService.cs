using MongoDB.Driver;
using OnewheroBayVisitorManagement.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnewheroBayVisitorManagement.Services
{
    public class MongoDBService
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<Visitor> _visitorsCollection;
        private readonly IMongoCollection<Event> _eventsCollection;
        private readonly IMongoCollection<Booking> _bookingsCollection;
        private readonly IMongoCollection<Attraction> _attractionsCollection;

        public MongoDBService()
        {
            // Replace with your actual connection string
            string connectionString = "mongodb+srv://admin:Masrafe123@cluster0.vwgkfxt.mongodb.net/?appName=Cluster0";
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase("OnewheroBayPark");

            _visitorsCollection = _database.GetCollection<Visitor>("Visitors");
            _eventsCollection = _database.GetCollection<Event>("Events");
            _bookingsCollection = _database.GetCollection<Booking>("Bookings");
            _attractionsCollection = _database.GetCollection<Attraction>("Attractions");
        }

        // VISITOR OPERATIONS
        public async Task<List<Visitor>> GetAllVisitorsAsync()
        {
            return await _visitorsCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Visitor> GetVisitorByIdAsync(string id)
        {
            return await _visitorsCollection.Find(v => v.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Visitor> GetVisitorByEmailAsync(string email)
        {
            return await _visitorsCollection.Find(v => v.Email == email).FirstOrDefaultAsync();
        }

        public async Task CreateVisitorAsync(Visitor visitor)
        {
            await _visitorsCollection.InsertOneAsync(visitor);
        }

        public async Task UpdateVisitorAsync(string id, Visitor visitor)
        {
            await _visitorsCollection.ReplaceOneAsync(v => v.Id == id, visitor);
        }

        public async Task DeleteVisitorAsync(string id)
        {
            await _visitorsCollection.DeleteOneAsync(v => v.Id == id);
        }

        // EVENT OPERATIONS
        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _eventsCollection.Find(_ => true).ToListAsync();
        }

        public async Task<List<Event>> GetActiveEventsAsync()
        {
            return await _eventsCollection.Find(e => e.IsActive).ToListAsync();
        }

        public async Task<Event> GetEventByIdAsync(string id)
        {
            return await _eventsCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateEventAsync(Event eventItem)
        {
            await _eventsCollection.InsertOneAsync(eventItem);
        }

        public async Task UpdateEventAsync(string id, Event eventItem)
        {
            await _eventsCollection.ReplaceOneAsync(e => e.Id == id, eventItem);
        }

        public async Task DeleteEventAsync(string id)
        {
            await _eventsCollection.DeleteOneAsync(e => e.Id == id);
        }

        // BOOKING OPERATIONS
        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            return await _bookingsCollection.Find(_ => true).ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsByVisitorIdAsync(string visitorId)
        {
            return await _bookingsCollection.Find(b => b.VisitorId == visitorId).ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsByEventIdAsync(string eventId)
        {
            return await _bookingsCollection.Find(b => b.EventId == eventId).ToListAsync();
        }

        public async Task CreateBookingAsync(Booking booking)
        {
            await _bookingsCollection.InsertOneAsync(booking);
        }

        public async Task UpdateBookingAsync(string id, Booking booking)
        {
            await _bookingsCollection.ReplaceOneAsync(b => b.Id == id, booking);
        }

        public async Task DeleteBookingAsync(string id)
        {
            await _bookingsCollection.DeleteOneAsync(b => b.Id == id);
        }

        // ATTRACTION OPERATIONS
        public async Task<List<Attraction>> GetAllAttractionsAsync()
        {
            return await _attractionsCollection.Find(_ => true).ToListAsync();
        }

        public async Task<List<Attraction>> GetActiveAttractionsAsync()
        {
            return await _attractionsCollection.Find(a => a.IsActive).ToListAsync();
        }

        public async Task CreateAttractionAsync(Attraction attraction)
        {
            await _attractionsCollection.InsertOneAsync(attraction);
        }

        public async Task UpdateAttractionAsync(string id, Attraction attraction)
        {
            await _attractionsCollection.ReplaceOneAsync(a => a.Id == id, attraction);
        }

        // ANALYTICS
        public async Task<int> GetTotalVisitorsCountAsync()
        {
            return (int)await _visitorsCollection.CountDocumentsAsync(_ => true);
        }

        public async Task<int> GetTotalBookingsCountAsync()
        {
            return (int)await _bookingsCollection.CountDocumentsAsync(_ => true);
        }

        public async Task<int> GetActiveEventsCountAsync()
        {
            return (int)await _eventsCollection.CountDocumentsAsync(e => e.IsActive);
        }

        internal async Task DeleteAttractionAsync(string? attractionId)
        {
            throw new NotImplementedException();
        }

        internal async Task GetAllAdminsAsync()
        {
            throw new NotImplementedException();
        }

    }   
}