using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace OnewheroBayVMS
{
    public class MongoService
    {
        private readonly IMongoCollection<User>? _users;

        public MongoService()
        {
            try
            {
                var client = new MongoClient("mongodb://localhost:27017");
                var database = client.GetDatabase("OnewheroBayVMS"); // Placeholder name
                _users = database.GetCollection<User>("users");
            }
            catch
            {
                // Database not available yet
                _users = null;
            }
        }

        public User? Authenticate(string username, string password)
        {
            if (_users == null)
            {
                // Database not ready, return null or throw as needed
                return null;
            }

            // In production, use hashed passwords!
            return _users.Find(u => u.Username == username && u.Password == password).FirstOrDefault();
        }
    }
}
