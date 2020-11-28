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

        public User GetUserByEmail(string email)
        {
            return _users.FirstOrDefault(user => user.Email.Equals(email));
        }

        public bool StoreUser(User user)
        {
            _users.Add(user);
            return true;
        }

        public bool UpdateSensitiveUserData(User user)
        {
            User foundUser = GetUserByEmail(user.Email);

            if (foundUser == null)
                return false;

            foundUser.PhoneNumberEncrypted = user.PhoneNumberEncrypted;
            foundUser.PhoneNumberNonce = user.PhoneNumberNonce;
            foundUser.CreditCardEncrypted = user.CreditCardEncrypted;
            foundUser.CreditCardNonce = user.CreditCardNonce;

            return true;
        }
    }
}
