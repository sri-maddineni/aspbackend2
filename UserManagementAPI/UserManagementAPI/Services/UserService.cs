using MongoDB.Driver;
using UserManagementAPI.Models;
using BCrypt.Net;

namespace UserManagementAPI.Services
{
	public class UserService
	{
		private readonly IMongoCollection<User> _users;

		public UserService(IUserDatabaseSettings settings, IMongoClient mongoClient)
		{
			var database = mongoClient.GetDatabase(settings.DatabaseName);
			_users = database.GetCollection<User>(settings.UsersCollectionName);
		}

		public User GetByEmail(string email) =>
			_users.Find<User>(user => user.Email == email).FirstOrDefault();

		public User Create(User user)
		{
			//user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
			_users.InsertOne(user);
			return user;
		}

		public bool VerifyPassword(string inputPassword, string storedHash) =>
			BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
	}
}
