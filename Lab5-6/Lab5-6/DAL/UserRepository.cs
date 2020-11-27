using Lab5_6.Models;
using System.Collections.Generic;
using System.Linq;

namespace Lab5_6.DAL
{
    public class UserRepository : IUserRepository
    {
        private readonly List<User> _users;

        public UserRepository()
        {
            _users = new List<User>();
        }

        public List<User> GetAllUsers() => _users;

        public User GetUserByCreds(string email, string passwordHash)
        {
            return _users.FirstOrDefault(user => user.Email.Equals(email) && user.PasswordHash.Equals(passwordHash));
        }

        public bool StoreUser(User user)
        {
            _users.Add(user);
            return true;
        }
    }
}
